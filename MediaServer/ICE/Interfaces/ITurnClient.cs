using MediaServer.ICE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.ICE.Interfaces
{
    public interface ITurnClient
    {
        Task<IEnumerable<TURNAllocation>> AllocateChannelsAsync(
            string turnServer = null,
            string username = null,
            string password = null,
            CancellationToken cancellationToken = default);
    }

}
