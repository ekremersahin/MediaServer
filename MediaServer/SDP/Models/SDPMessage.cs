using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.SDP.Models
{
    public class SDPMessage
    {
        public string Type { get; set; } // offer, answer, candidate vb.
        public string Sdp { get; set; }
        public string ClientId { get; set; }

        public string Payload { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    }
}
