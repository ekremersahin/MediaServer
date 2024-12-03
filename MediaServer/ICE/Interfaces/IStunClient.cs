using MediaServer.ICE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.ICE.Interfaces
{
    public interface IStunClient
    {
        Task<STUNResponse> GetPublicAddressAsync(
            string stunServer = "stun.l.google.com",
            int port = 19302,
            CancellationToken cancellationToken = default);
    }

}
