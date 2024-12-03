using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Media.Models
{
    public class MediaDescription
    {
        public string Type { get; set; }
        public int Port { get; set; }
        public string Protocol { get; set; }
        public List<int> FormatIds { get; set; }
        public Dictionary<string, string> Attributes { get; set; }

        public int? Width { get; set; }
        public int? Height { get; set; }

    }

}
