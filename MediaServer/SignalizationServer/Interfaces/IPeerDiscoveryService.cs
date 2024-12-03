using MediaServer.SignalizationServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.SignalizationServer.Interfaces
{
    public interface IPeerDiscoveryService
    {
        Task<IEnumerable<PeerMetadata>> GetAvailablePeersAsync(
            MediaType? mediaType = null,
            int? maxBandwidth = null);
        Task<PeerMetadata> GetPeerMetadataAsync(string clientId);
        Task UpdatePeerMetadataAsync(PeerMetadata metadata);
        // Task<bool> IsPeerAvailableAsync(string clientId);

        // Event bazlı peer durumu bildirimi
        event Func<string, PeerStatus, Task> PeerStatusChanged;

    }
}
