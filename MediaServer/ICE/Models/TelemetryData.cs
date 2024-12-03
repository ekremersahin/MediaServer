using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.ICE.Models
{
    public class TelemetryData
    {
        public string CandidateId { get; set; }
        public double Priority { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public Dictionary<string, string> Metadata { get; set; }

    }
}
