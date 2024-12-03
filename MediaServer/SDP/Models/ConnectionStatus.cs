using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.SDP.Models
{
    public enum ConnectionStatus
    {
        NotInitialized,
        Initializing,
        Connected,
        Failed,
        Closed
    }
}
