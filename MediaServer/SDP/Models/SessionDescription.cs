using MediaServer.Media.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.SDP.Models
{
    public class SessionDescription
    {
        public string Version { get; set; }
        public Origin Origin { get; set; }
        public string SessionName { get; set; }
        public ConnectionInfo Connection { get; set; }
        public TimeDescription Time { get; set; }
        public List<MediaDescription> Media { get; set; }
        public Dictionary<string, string> Attributes { get; set; }
    }

}
