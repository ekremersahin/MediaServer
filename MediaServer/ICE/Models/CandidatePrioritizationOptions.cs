using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.ICE.Models
{
    public class CandidatePrioritizationOptions
    {
        public int BaseHostPriority { get; set; } = 126;
        public int BaseServerReflexivePriority { get; set; } = 100;
        public int BasePeerReflexivePriority { get; set; } = 75;
        public int BaseRelayedPriority { get; set; } = 50;
        public double LatencyWeightFactor { get; set; } = 0.3;
        public double BandwidthWeightFactor { get; set; } = 0.3;
        public double PacketLossWeightFactor { get; set; } = 0.2;
        public double GeographicalProximityWeightFactor { get; set; } = 0.2;
    }
}
