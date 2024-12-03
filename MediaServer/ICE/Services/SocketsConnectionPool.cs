
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace MediaServer.ICE.Services
{
    internal class SocketsConnectionPool : IDisposable
    {
        private readonly ConcurrentBag<Socket> _sockets;
        private readonly int _maxPoolSize;
        private readonly SemaphoreSlim _poolSemaphore;

        public SocketsConnectionPool(int maxPoolSize)
        {
            _maxPoolSize = maxPoolSize;
            _sockets = new ConcurrentBag<Socket>();
            _poolSemaphore = new SemaphoreSlim(maxPoolSize, maxPoolSize);
        }

        public async ValueTask<Socket> GetSocketAsync()
        {
            await _poolSemaphore.WaitAsync();

            if (_sockets.TryTake(out var socket) && IsSocketHealthy(socket))
            {
                return socket;
            }

            return new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        public ValueTask ReturnSocketAsync(Socket socket)
        {
            if (IsSocketHealthy(socket) && _sockets.Count < _maxPoolSize)
            {
                _sockets.Add(socket);
            }
            else
            {
                socket.Dispose();
            }

            _poolSemaphore.Release();
            return ValueTask.CompletedTask;
        }

        private bool IsSocketHealthy(Socket socket)
        {
            return socket != null && socket.Connected && !socket.Poll(1, SelectMode.SelectError);
        }

        public void Dispose()
        {
            foreach (var socket in _sockets)
            {
                socket?.Dispose();
            }
            _poolSemaphore.Dispose();
        }
    }
}