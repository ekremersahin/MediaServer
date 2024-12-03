using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Media.Models
{
    public class MediaProcessResult
    {
        public string MediaType { get; set; }
        public bool Success { get; set; }
        public int Port { get; set; }
        public string Protocol { get; set; }
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
        public List<string> Codecs { get; set; } = new List<string>();
    }

}
