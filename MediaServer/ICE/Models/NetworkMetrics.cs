using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.ICE.Models
{
    public class NetworkMetrics
    {
        public double Latency { get; set; }      // ms
        public long Bandwidth { get; set; }      // bps
        public double PacketLoss { get; set; }   // %
    }
}
