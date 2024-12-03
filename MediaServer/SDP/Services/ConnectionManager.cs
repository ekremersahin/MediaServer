using MediaServer.SDP.Interfaces;
using MediaServer.SDP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.SDP.Services
{
    public class ConnectionManager : IConnectionManager
    {
        public async Task InitializeAsync(Origin origin)
        { }

        public async Task<ConnectionStatus> EstablishConnectionAsync(ConnectionInfo connection)
            => ConnectionStatus.Connected;
    }
}
