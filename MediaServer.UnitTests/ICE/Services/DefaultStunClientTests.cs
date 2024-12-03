using MediaServer.ICE.Exc;
using MediaServer.ICE.Interfaces;
using MediaServer.ICE.Models;
using MediaServer.ICE.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Net.Sockets;
using Xunit;
using Xunit.Abstractions;

namespace MediaServer.UnitTests.ICE.Services
{
    public class DefaultStunClientTests
    {
        private readonly Mock<ILogger<DefaultStunClient>> _loggerMock;
        private readonly Mock<StunClientOptions> _optionsMock;
        private readonly StunClientOptions _options;
        private readonly ITestOutputHelper _output;

        public DefaultStunClientTests(ITestOutputHelper output)
        {
            _output = output;
            _loggerMock = new Mock<ILogger<DefaultStunClient>>();
            _options = new StunClientOptions { MaxPoolSize = 10 };
            _optionsMock = new Mock<StunClientOptions>();
            _optionsMock.Setup(x => x).Returns(_options);

            // Log mesajlarını test çıktısına yönlendir
            _loggerMock.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            )).Callback(new InvocationAction(invocation =>
            {
                var logLevel = (LogLevel)invocation.Arguments[0];
                var state = invocation.Arguments[2];
                var exception = (Exception)invocation.Arguments[3];
                _output.WriteLine($"[{logLevel}] {state} {exception?.Message ?? ""}");
            }));
        }

        [Fact]
        public async Task GetPublicAddressAsync_WithValidServer_ReturnsValidResponse()
        {
            // Arrange
            var client = new DefaultStunClient(_loggerMock.Object, (Microsoft.Extensions.Options.IOptions<StunClientOptions>)_optionsMock.Object);

            // Act
            var result = await client.GetPublicAddressAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual("0.0.0.0", result.PublicIpAddress);
            Assert.True(result.PublicPort > 0);

            // Log the STUN response details
            _output.WriteLine($"Public IP: {result.PublicIpAddress}");
            _output.WriteLine($"Public Port: {result.PublicPort}");
            _output.WriteLine($"NAT Type: {result.NatType}");
            _output.WriteLine($"Is NAT Detected: {result.IsNatDetected}");
        }

        [Fact]
        public async Task GetPublicAddressAsync_WithInvalidServer_ShouldThrowStunConnectionException()
        {
            // Arrange
            var client = new DefaultStunClient(_loggerMock.Object, (Microsoft.Extensions.Options.IOptions<StunClientOptions>)_optionsMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<StunConnectionException>(() =>
                client.GetPublicAddressAsync("invalid.stun.server", 19302));
            Assert.Contains("Failed to connect to STUN server", ex.Message);
        }

        [Fact]
        public async Task GetPublicAddressAsync_ExceedsRateLimit_ShouldThrowStunException()
        {
            // Arrange
            var client = new DefaultStunClient(_loggerMock.Object, (Microsoft.Extensions.Options.IOptions<StunClientOptions>)_optionsMock.Object);
            var tasks = new List<Task>();

            // Act & Assert
            for (int i = 0; i < 70; i++) // Rate limit is 60 per minute
            {
                tasks.Add(client.GetPublicAddressAsync());
            }

            var ex = await Assert.ThrowsAsync<StunException>(() =>
                Task.WhenAll(tasks));
            Assert.True(ex.Message.Contains("Rate limit exceeded") || ex.InnerException.Message.Contains("Rate limit exceeded"),
                $"Expected rate limit exception but got: {ex.Message} |  {ex.InnerException.Message}");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public async Task GetPublicAddressAsync_WithInvalidServerAddress_ShouldThrowArgumentException(string server)
        {
            // Arrange
            var client = new DefaultStunClient(_loggerMock.Object, (Microsoft.Extensions.Options.IOptions<StunClientOptions>)_optionsMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                client.GetPublicAddressAsync(server));
            Assert.Contains("STUN server address cannot be empty", ex.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(65536)]
        public async Task GetPublicAddressAsync_WithInvalidPort_ShouldThrowArgumentOutOfRangeException(int port)
        {
            // Arrange
            var client = new DefaultStunClient(_loggerMock.Object, (Microsoft.Extensions.Options.IOptions<StunClientOptions>)_optionsMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                client.GetPublicAddressAsync("stun.l.google.com", port));
            Assert.Contains("Port must be between 1 and 65535", ex.Message);
        }

        [Fact]
        public async Task GetPublicAddressAsync_WithCancellation_ThrowsOperationCanceledException()
        {
            // Arrange
            var client = new DefaultStunClient(_loggerMock.Object, (Microsoft.Extensions.Options.IOptions<StunClientOptions>)_optionsMock.Object);
            using var cts = new CancellationTokenSource();

            // Act
            cts.Cancel();

            // Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                client.GetPublicAddressAsync(cancellationToken: cts.Token));
        }

        [Fact]
        public async Task GetPublicAddressAsync_WithMultipleRealServers_ShouldSucceed()
        {
            // Arrange
            var client = new DefaultStunClient(_loggerMock.Object, (Microsoft.Extensions.Options.IOptions<StunClientOptions>)_optionsMock.Object);
            var servers = new[]
            {
                ("stun.l.google.com", 19302),
                ("stun1.l.google.com", 19302),
                ("stun2.l.google.com", 19302)
            };

            foreach (var (server, port) in servers)
            {
                // Act
                var result = await client.GetPublicAddressAsync(server, port);

                // Assert
                Assert.NotNull(result);
                Assert.NotEqual("0.0.0.0", result.PublicIpAddress);
                Assert.True(result.PublicPort > 0);
                Assert.True(IPAddress.TryParse(result.PublicIpAddress, out _),
                    $"Invalid IP address format: {result.PublicIpAddress}");

                // Log for debugging
                _output.WriteLine($"\nServer: {server}:{port}");
                _output.WriteLine($"Public IP: {result.PublicIpAddress}");
                _output.WriteLine($"Public Port: {result.PublicPort}");
                _output.WriteLine($"NAT Type: {result.NatType}");
            }
        }

        [Fact]
        public async Task GetPublicAddressAsync_WithRetry_EventuallySucceeds()
        {
            // Arrange
            var client = new DefaultStunClient(_loggerMock.Object, (Microsoft.Extensions.Options.IOptions<StunClientOptions>)_optionsMock.Object);

            // Act
            var result = await client.GetPublicAddressAsync("stun.l.google.com", 19302);

            // Assert
            Assert.NotNull(result);
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Retry attempt")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtMost(3));
        }

        [Fact]
        public async Task GetPublicAddressAsync_WithMultipleServers_ShowsAllResponses()
        {
            // Arrange
            var client = new DefaultStunClient(_loggerMock.Object, (Microsoft.Extensions.Options.IOptions<StunClientOptions>)_optionsMock.Object);
            var servers = new[]
            {
                ("stun.l.google.com", 19302),
                ("stun1.l.google.com", 19302),
                ("stun2.l.google.com", 19302)
            };

            // Act & Assert
            foreach (var (server, port) in servers)
            {
                try
                {
                    var result = await client.GetPublicAddressAsync(server, port);
                    _output.WriteLine($"\nServer: {server}:{port}");
                    _output.WriteLine($"Public IP: {result.PublicIpAddress}");
                    _output.WriteLine($"Public Port: {result.PublicPort}");
                    _output.WriteLine($"NAT Type: {result.NatType}");
                    _output.WriteLine($"Is NAT Detected: {result.IsNatDetected}");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"\nServer: {server}:{port}");
                    _output.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        [Fact]
        public async Task GetPublicAddressAsync_WithRetry_ShowsRetryAttempts()
        {
            // Arrange
            var client = new DefaultStunClient(_loggerMock.Object, (Microsoft.Extensions.Options.IOptions<StunClientOptions>)_optionsMock.Object);

            // Act
            var result = await client.GetPublicAddressAsync("stun.invalid.server", 19302);

            // Verify that retry attempts were logged
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Retry attempt")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Between(1, 3, Moq.Range.Inclusive));
        }
    }
}