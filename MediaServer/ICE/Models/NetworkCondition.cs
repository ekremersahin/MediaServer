using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.ICE.Models
{
    public class NetworkCondition
    {
        public double Latency { get; set; }
        public long Bandwidth { get; set; }
        public double PacketLoss { get; set; }
    }
}
