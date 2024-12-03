using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.SignalizationServer.Models
{
    public class ConnectionCapabilities
    {
        public List<string> SupportedCodecs { get; set; }
        public List<string> SupportedProtocols { get; set; }
        public int MaxBandwidth { get; set; }
        public bool SupportsEncryption { get; set; }
    }
}
