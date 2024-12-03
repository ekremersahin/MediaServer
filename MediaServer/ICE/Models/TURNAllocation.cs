using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.ICE.Models
{
    public class TURNAllocation
    {
        public string RelayedAddress { get; set; }
        public int RelayedPort { get; set; }
        public string Username { get; set; }
        public string Realm { get; set; }
        public DateTime AllocationExpiration { get; set; }
    }
}
