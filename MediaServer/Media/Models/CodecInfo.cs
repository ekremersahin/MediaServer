using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Media.Models
{

    public class CodecInfo
    {
        public string Name { get; set; }
        public int PayloadType { get; set; }
        public string Description { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
    }
}
