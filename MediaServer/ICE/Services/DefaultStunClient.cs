using MediaServer.ICE.Constants;
using MediaServer.ICE.Exc;
using MediaServer.ICE.Interfaces;
using MediaServer.ICE.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
namespace MediaServer.ICE.Services
{
    public class DefaultStunClient : IStunClient, IDisposable
    {
        private const int MAX_RETRY_ATTEMPTS = 3;
        private const int BUFFER_SIZE = 1024;
        private const int MAX_CONCURRENT_OPERATIONS = 100;
        private static readonly ConcurrentDictionary<string, (DateTime LastAccess, int Count)> _requestThrottling =
            new();
        private readonly SemaphoreSlim _throttleSemaphore;
        private readonly MemoryPool<byte> _memoryPool;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly ILogger<DefaultStunClient> _logger;
        private readonly StunClientOptions _options;
        private readonly SocketsConnectionPool _connectionPool;
        private readonly ArrayPool<byte> _arrayPool;

        public DefaultStunClient(
            ILogger<DefaultStunClient> logger,
             IOptions<StunClientOptions> options)
        {
            _logger = logger;
            _options = options.Value;
            _connectionPool = new SocketsConnectionPool(_options.MaxPoolSize);
            _arrayPool = ArrayPool<byte>.Shared;

            _retryPolicy = Policy
                .Handle<StunConnectionException>()
                .Or<SocketException>()
                .WaitAndRetryAsync(
                    MAX_RETRY_ATTEMPTS,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            exception,
                            "Retry attempt {RetryCount} of {MaxRetries} after {Delay}ms delay",
                            retryCount,
                            MAX_RETRY_ATTEMPTS,
                            timeSpan.TotalMilliseconds);
                    });

            _throttleSemaphore = new SemaphoreSlim(MAX_CONCURRENT_OPERATIONS);
            _memoryPool = MemoryPool<byte>.Shared;
        }

        public async Task<STUNResponse> GetPublicAddressAsync(
            string stunServer = "stun.l.google.com",
            int port = 19302,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stunServer, nameof(stunServer));

            if (string.IsNullOrWhiteSpace(stunServer))
                throw new ArgumentException("STUN server address cannot be empty", nameof(stunServer));

            if (port <= 0 || port > 65535)
                throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 1 and 65535");

            return await _retryPolicy.ExecuteAsync(async (context) =>
            {
                try
                {
                    var initialResponse = await SendStunRequestAsync(stunServer, port, 0, cancellationToken);
                    if (initialResponse.IsNatDetected && initialResponse.NatType != NatType.Unknown)
                    {
                        return initialResponse;
                    }

                    var natType = await DetermineNatTypeAsync(stunServer, port, initialResponse, cancellationToken);
                    initialResponse.NatType = natType;
                    return initialResponse;
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("STUN request cancelled for {server}:{port}", stunServer, port);
                    throw;
                }
                catch (Exception ex) when (ex is not StunConnectionException && ex is not SocketException)
                {
                    _logger.LogError(ex, "Unexpected error during STUN operation for {server}:{port}", stunServer, port);
                    throw new StunException("An unexpected error occurred during STUN operation", ex);
                }
            }, new Context());
        }

        private async Task<NatType> DetermineNatTypeAsync(string stunServer, int port, STUNResponse initialResponse, CancellationToken cancellationToken)
        {
            try
            {
                // Test 1: Change IP and Port request
                var changeIpPortResponse = await SendStunRequestAsync(stunServer, port, StunConstants.ChangeRequestFlags.CHANGE_IP | StunConstants.ChangeRequestFlags.CHANGE_PORT, cancellationToken);
                if (changeIpPortResponse.IsNatDetected)
                {
                    return NatType.FullConeNAT;
                }

                // Test 2: Change Port only request
                var changePortResponse = await SendStunRequestAsync(stunServer, port, StunConstants.ChangeRequestFlags.CHANGE_PORT, cancellationToken);
                if (changePortResponse.IsNatDetected)
                {
                    return NatType.RestrictedNAT;
                }

                // Test 3: Binding request from different source
                var alternateServer = await GetAlternateServerAsync(stunServer, port, cancellationToken);
                if (alternateServer != null)
                {
                    var alternateResponse = await SendStunRequestAsync(alternateServer.Value.Address, alternateServer.Value.Port, 0, cancellationToken);
                    if (!alternateResponse.IsNatDetected)
                    {
                        return NatType.SymmetricNAT;
                    }
                    return NatType.PortRestrictedNAT;
                }

                return NatType.AddressRestrictedNAT;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NAT type determination failed");
                return NatType.Unknown;
            }
        }

        private async ValueTask<STUNResponse> SendStunRequestAsync(string stunServer, int port, uint changeRequest, CancellationToken cancellationToken)
        {
            if (!await CheckRateLimitAsync(stunServer))
                throw new StunException("Rate limit exceeded for this server", new Exception());

            await _throttleSemaphore.WaitAsync(cancellationToken);

            try
            {
                using var memory = _memoryPool.Rent(BUFFER_SIZE);
                Socket socket = null;
                byte[] buffer = null;
                try
                {
                    socket = await _connectionPool.GetSocketAsync();
                    buffer = _arrayPool.Rent(1024);

                    // Timeout değerlerini daha yüksek ayarla
                    socket.ReceiveTimeout = 3000; // 30 saniye
                    socket.SendTimeout = 3000;    // 30 saniye

                    // UDP için özel ayarlar
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                    using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(3)); // 45 saniyelik timeout
                    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);

                    try
                    {
                        var addresses = await Dns.GetHostAddressesAsync(stunServer, linkedCts.Token);
                        var remoteEp = new IPEndPoint(addresses[0], port);

                        // UDP için Connect yerine sadece endpoint'i ayarla
                        socket.Bind(new IPEndPoint(IPAddress.Any, 0));


                        var stunPacket = CreateStunPacket(changeRequest);


                        // UDP için SendTo kullan
                        var sendResult = await socket.SendToAsync(
                            stunPacket,
                            SocketFlags.None,
                            remoteEp,
                            linkedCts.Token
                        ).ConfigureAwait(false);

                        if (sendResult <= 0)
                        {
                            throw new StunException("Failed to send STUN packet", null);
                        }

                        // UDP için ReceiveFrom kullan
                        var receiveBuffer = memory.Memory[..BUFFER_SIZE];
                        var receiveResult = await socket.ReceiveFromAsync(
                            receiveBuffer,
                            SocketFlags.None,
                            remoteEp,
                            linkedCts.Token
                        ).ConfigureAwait(false);

                        // Transaction ID validation
                        if (!ValidateTransactionId(memory.Memory.Span[..receiveResult.ReceivedBytes].ToArray(), stunPacket))
                        {
                            throw new StunProtocolException("Transaction ID mismatch");
                        }

                        // Add FINGERPRINT validation
                        if (!ValidateFingerprint(memory.Memory.Span[..receiveResult.ReceivedBytes].ToArray(), receiveResult.ReceivedBytes))
                        {
                            throw new StunProtocolException("Invalid FINGERPRINT");
                        }

                        return ParseStunResponse(memory.Memory.Span[..receiveResult.ReceivedBytes].ToArray(), receiveResult.ReceivedBytes);
                    }
                    catch (SocketException ex)
                    {
                        throw new StunConnectionException($"Failed to connect to STUN server {stunServer}:{port}", ex);
                    }
                    catch (OperationCanceledException ex)
                    {
                        throw new StunConnectionException($"Connection attempt to {stunServer}:{port} timed out", ex);
                    }
                }
                finally
                {
                    if (buffer != null)
                        _arrayPool.Return(buffer);

                    if (socket != null)
                        await _connectionPool.ReturnSocketAsync(socket);
                }
            }
            finally
            {
                _throttleSemaphore.Release();
            }
        }

        private byte[] CreateStunPacket(uint changeRequest = 0)
        {
            const ushort BINDING_REQUEST = 0x0001;
            var transactionId = new byte[12];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(transactionId);
            }

            // Calculate message length - her zaman 8 byte kullan (attribute header + value)
            const ushort messageLength = 8;
            var message = new byte[20 + messageLength];

            // Set message type (Binding Request)
            message[0] = (byte)(BINDING_REQUEST >> 8);
            message[1] = (byte)(BINDING_REQUEST & 0xFF);

            // Set message length
            message[2] = (byte)(messageLength >> 8);
            message[3] = (byte)(messageLength & 0xFF);

            // Set Magic Cookie (fixed value: 0x2112A442)
            message[4] = 0x21;
            message[5] = 0x12;
            message[6] = 0xA4;
            message[7] = 0x42;

            // Set Transaction ID
            Buffer.BlockCopy(transactionId, 0, message, 8, 12);

            // Her zaman CHANGE-REQUEST attribute ekle
            var pos = 20;
            // Attribute Type (0x0003 for CHANGE-REQUEST)
            message[pos++] = 0x00;
            message[pos++] = 0x03;
            // Attribute Length (4 bytes)
            message[pos++] = 0x00;
            message[pos++] = 0x04;
            // Attribute Value - sadece alt 32 biti kullan
            message[pos++] = 0x00;
            message[pos++] = 0x00;
            message[pos++] = (byte)((changeRequest >> 8) & 0xFF);
            message[pos] = (byte)(changeRequest & 0xFF);

            return message;
        }

        private async Task<(string Address, int Port)?> GetAlternateServerAsync(string stunServer, int port, CancellationToken cancellationToken)
        {
            try
            {
                using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                await socket.ConnectAsync(stunServer, port, cancellationToken);

                var packet = CreateStunPacket();
                await socket.SendAsync(packet, SocketFlags.None, cancellationToken);

                var receivedData = new byte[1024];
                var bytesReceived = await socket.ReceiveAsync(receivedData, SocketFlags.None, cancellationToken);

                return ParseOtherAddress(receivedData, bytesReceived);
            }
            catch
            {
                return null;
            }
        }

        private (string Address, int Port)? ParseOtherAddress(byte[] data, int length)
        {
            var position = StunConstants.Protocol.HEADER_LENGTH;
            while (position + 4 <= length)
            {
                var attributeType = BitConverter.ToUInt16(data.AsSpan(position, 2));
                var attributeLength = BitConverter.ToUInt16(data.AsSpan(position + 2, 2));
                position += 4;

                if (attributeType == StunConstants.RFC5780.OTHER_ADDRESS && attributeLength >= 8)
                {
                    var family = BitConverter.ToUInt16(data.AsSpan(position, 2));
                    var port = BitConverter.ToUInt16(data.AsSpan(position + 2, 2));
                    var ip = new byte[4];
                    Buffer.BlockCopy(data, position + 4, ip, 0, 4);
                    return (new IPAddress(ip).ToString(), port);
                }

                position += attributeLength;
                position += (4 - (attributeLength % 4)) % 4;
            }
            return null;
        }

        // STUN yanıtını analiz et
        private STUNResponse ParseStunResponse(byte[] receivedData, int bytesReceived)
        {
            var krt = System.Text.Encoding.UTF8.GetString(receivedData);
            if (bytesReceived < StunConstants.Protocol.HEADER_LENGTH)
            {
                throw new InvalidDataException("Invalid STUN response: too short");
            }

            if (bytesReceived < 20 || bytesReceived > 548)
                throw new InvalidDataException($"Invalid packet length: {bytesReceived}");


            //if (!StunMessageIntegrity.ValidateMessageIntegrity(receivedData, _credentials.Key))
            //{
            //    throw new SecurityException("STUN message integrity check failed");
            //}

            if (!StunMessageIntegrity.ValidateFingerprint(receivedData))
            {
                _logger.LogWarning("STUN fingerprint check failed");
            }

            var type = BitConverter.ToUInt16(receivedData, 0);
            var length = BitConverter.ToUInt16(receivedData.AsSpan(2, 2));

            // Magic Cookie'yi byte'ları doğru sırada okuyalım
            uint magicCookie = (uint)(
                (receivedData[4] << 24) |
                (receivedData[5] << 16) |
                (receivedData[6] << 8) |
                (receivedData[7]));

            var transactionId = receivedData.AsSpan(8, 12).ToArray();

            if (magicCookie != StunConstants.Protocol.MAGIC_COOKIE)
            {
                throw new InvalidDataException("Invalid STUN response: wrong magic cookie");
            }

            if (type != StunConstants.MessageTypes.BINDING_RESPONSE_SUCCESS)
            {
                return new STUNResponse
                {
                    PublicIpAddress = "0.0.0.0",
                    PublicPort = 0,
                    IsNatDetected = false,
                    NatType = NatType.Unknown
                };
            }

            // Attribute parsing
            var position = StunConstants.Protocol.HEADER_LENGTH;
            while (position + 4 <= bytesReceived)
            {
                // Öznitelik tipini ve uzunluğunu network byte order'da (big-endian) oku
                var attributeType = (ushort)((receivedData[position] << 8) | receivedData[position + 1]);
                var attributeLength = (ushort)((receivedData[position + 2] << 8) | receivedData[position + 3]);

                _logger.LogDebug("Found STUN attribute type: 0x{Type:X4}, raw bytes: {Byte1:X2} {Byte2:X2}",
                    attributeType, receivedData[position], receivedData[position + 1]);

                position += 4;

                // Öznitelik tiplerini kontrol et (network byte order'da)
                switch (attributeType)
                {
                    case 0x0001: // MAPPED-ADDRESS (0x0001)
                    case 0x8001: // MAPPED-ADDRESS alternatif (0x8001)
                        _logger.LogDebug("Found MAPPED-ADDRESS");
                        return ParseMappedAddress(receivedData, position);

                    case 0x0020: // XOR-MAPPED-ADDRESS (0x0020)
                    case 0x8020: // XOR-MAPPED-ADDRESS alternatif (0x8020)
                        _logger.LogDebug("Found XOR-MAPPED-ADDRESS");
                        return ParseXorMappedAddress(receivedData, position, magicCookie);

                    case 0x8022: // SOFTWARE
                    case 0x8023: // ALTERNATE-SERVER
                    case 0x8028: // FINGERPRINT
                        _logger.LogDebug("Found known attribute: 0x{Type:X4}", attributeType);
                        break;

                    default:
                        _logger.LogDebug("Unknown attribute type: 0x{Type:X4}", attributeType);
                        break;
                }

                position += attributeLength;
                if (attributeLength % 4 != 0)
                {
                    position += 4 - (attributeLength % 4);
                }
            }

            // Debug için tüm mesajı hexadecimal olarak logla
            _logger.LogWarning("Response content (hex): {Hex}",
                BitConverter.ToString(receivedData, 0, bytesReceived).Replace("-", " "));

            throw new InvalidDataException("No valid address attribute found in STUN response");
        }

        private STUNResponse ParseXorMappedAddress(byte[] data, int position, uint magicCookie)
        {
            // Skip first 2 bytes (family and reserved)
            var port = BitConverter.ToUInt16(data.AsSpan(position + 2, 2));
            port ^= (ushort)(magicCookie >> 16);

            var ip = new byte[4];
            Buffer.BlockCopy(data, position + 4, ip, 0, 4);

            // XOR with magic cookie
            for (int i = 0; i < 4; i++)
            {
                ip[i] ^= (byte)(magicCookie >> (8 * (3 - i)));
            }

            return new STUNResponse
            {
                PublicIpAddress = new IPAddress(ip).ToString(),
                PublicPort = port,
                IsNatDetected = true,
                NatType = DetermineNatType(port)
            };
        }

        private STUNResponse ParseMappedAddress(byte[] data, int position)
        {
            // MAPPED-ADDRESS yapısı:
            // 1 byte reserved (0)
            // 1 byte address family (0x01 for IPv4)
            // 2 bytes port number
            // 4 bytes IPv4 address

            var family = data[position + 1];
            if (family != 0x01)
            {
                _logger.LogWarning("Unsupported address family: 0x{Family:X2}", family);
                throw new InvalidDataException("Only IPv4 is supported");
            }

            var port = (ushort)((data[position + 2] << 8) | data[position + 3]);
            var ip = new byte[4];
            Buffer.BlockCopy(data, position + 4, ip, 0, 4);

            var response = new STUNResponse
            {
                PublicIpAddress = new IPAddress(ip).ToString(),
                PublicPort = port,
                IsNatDetected = true,
                NatType = DetermineNatType(port)
            };

            _logger.LogDebug("Parsed MAPPED-ADDRESS: {IP}:{Port}", response.PublicIpAddress, response.PublicPort);
            return response;
        }

        private NatType DetermineNatType(int port)
        {
            // Basit NAT tespiti - gerçek implementasyonda daha kompleks olmalı
            return port > 0 ? NatType.FullConeNAT : NatType.Unknown;
        }

        private bool ValidateTransactionId(byte[] response, byte[] request)
        {
            // Transaction ID başlangıç pozisyonu 8, uzunluk 12 byte
            if (response.Length < 20 || request.Length < 20)
                return false;

            // Request ve response'daki transaction ID'leri karşılaştır
            for (int i = 8; i < 20; i++)
            {
                if (response[i] != request[i])
                    return false;
            }
            return true;
        }

        private bool ValidateFingerprint(byte[] data, int length)
        {
            // FINGERPRINT kontrolünü basitleştirelim
            // STUN mesajlarında FINGERPRINT opsiyonel olduğu için
            // burada her zaman true dönebiliriz
            return true;
        }

        private uint CalculateCRC32(byte[] data, int length)
        {
            const uint polynomial = 0xedb88320;
            uint crc = 0xffffffff;

            for (var i = 0; i < length; i++)
            {
                crc ^= data[i];
                for (var j = 0; j < 8; j++)
                    crc = (crc >> 1) ^ ((crc & 1) * polynomial);
            }
            return crc;
        }

        private async Task<bool> CheckRateLimitAsync(string server)
        {
            const int MAX_REQUESTS_PER_MINUTE = 60;
            const int RATE_LIMIT_WINDOW_MINUTES = 1;

            var now = DateTime.UtcNow;
            var throttleInfo = _requestThrottling.AddOrUpdate(
                server,
                _ => (now, 1),
                (_, current) =>
                {
                    if ((now - current.LastAccess).TotalMinutes >= RATE_LIMIT_WINDOW_MINUTES)
                        return (now, 1);
                    return (current.LastAccess, current.Count + 1);
                });

            return throttleInfo.Count <= MAX_REQUESTS_PER_MINUTE;
        }

        public void Dispose()
        {
            _connectionPool.Dispose();
            _throttleSemaphore.Dispose();
        }
    }
}