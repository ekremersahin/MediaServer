using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.ICE.Models
{
    public enum NatType
    {
        Unknown,
        OpenInternet,
        FullConeNAT,
        SymmetricNAT,
        RestrictedNAT,
        PortRestrictedNAT,
        AddressRestrictedNAT
    }
}
