using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.ICE.Interfaces
{
    public enum CandidateType
    {
        Unknown,
        Host,           // Yerel ağ üzerindeki aday
        ServerReflexive, // STUN sunucusu üzerinden yansıyan aday
        PeerReflexive,   // Eş üzerinden yansıyan aday
        Relayed          // TURN sunucusu üzerinden aktarılan aday
    }
}
