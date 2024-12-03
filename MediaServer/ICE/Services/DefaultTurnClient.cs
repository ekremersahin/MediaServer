using MediaServer.ICE.Interfaces;
using MediaServer.ICE.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MediaServer.ICE.Services
{
    public class DefaultTurnClient : ITurnClient
    {
        private readonly ILogger<DefaultTurnClient> _logger;

        public DefaultTurnClient(ILogger<DefaultTurnClient> logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<TURNAllocation>> AllocateChannelsAsync(
            string turnServer = null,
            string username = null,
            string password = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // TURN sunucusuna bağlan
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    await socket.ConnectAsync(IPAddress.Parse(turnServer), 19302); // Varsayılan TURN portu

                    // TURN Allocate paketini oluştur
                    var turnPacket = CreateTurnAllocatePacket(username, password);

                    // TURN paketini gönder
                    await socket.SendAsync(turnPacket, SocketFlags.None);

                    // Yanıtı bekle
                    var receivedData = new byte[1024];
                    var bytesReceived = await socket.ReceiveAsync(receivedData, SocketFlags.None);

                    // Yanıtı analiz et
                    var turnResponse = ParseTurnResponse(receivedData, bytesReceived);

                    // Relay IP ve portu al
                    var relayedIpAddress = turnResponse.PublicIpAddress;
                    var relayedPort = turnResponse.PublicPort;

                    // Dönen TURNAllocation nesnesi oluştur
                    var allocation = new TURNAllocation
                    {
                        RelayedAddress = relayedIpAddress,
                        RelayedPort = relayedPort
                    };

                    return new List<TURNAllocation> { allocation }; // Tek bir kanal tahsis ediliyor
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "TURN sunucusuna bağlanırken hata oluştu");
                return Enumerable.Empty<TURNAllocation>();
            }
        }

        // TURN Allocate paketini oluştur
        private byte[] CreateTurnAllocatePacket(string username, string password)
        {
            // TURN başlık bilgileri
            var header = new byte[]
            {
                // Tip: Allocate Request (0x0003)
                0x00, 0x03,
                // Uzunluk: 0 byte (0x0000)
                0x00, 0x00,
                // Magic Cookie: 0x2112A442
                0x21, 0x12, 0xA4, 0x42,
                // Transaction ID (rastgele)
                // ...
            };

            // Transaction ID (16 byte) rastgele oluştur
            var transactionId = new byte[16];
            new Random().NextBytes(transactionId);

            // Kullanıcı adı ve şifreyi ekle
            // ... (username ve password'u STUN pakete eklemek için eklemeler yapılması gerekebilir)

            // TURN paketini oluştur
            var turnPacket = header.Concat(transactionId).ToArray();

            return turnPacket;
        }

        // TURN yanıtını analiz et
        private TURNResponse ParseTurnResponse(byte[] receivedData, int bytesReceived)
        {
            // TURN başlık bilgileri
            var type = BitConverter.ToUInt16(receivedData, 0);

            // TURN tipi kontrol et
            if (type != 0x0103) // Allocate Success
            {
                // Hatalı yanıt
                return new TURNResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Hatalı yanıt"
                };
            }

            // IP adresini ve portu al
            var publicIpAddress = $"{receivedData[8]}.{receivedData[9]}.{receivedData[10]}.{receivedData[11]}";
            var publicPort = (ushort)((receivedData[12] << 8) | receivedData[13]);

            return new TURNResponse
            {
                IsSuccess = true,
                PublicIpAddress = publicIpAddress,
                PublicPort = publicPort
            };
        }
    }
}