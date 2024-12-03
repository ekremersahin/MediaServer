using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.SDP.Models
{
    public class Origin
    {
        public string Username { get; set; }
        public string SessionId { get; set; }
        public string SessionVersion { get; set; }
        public string NetworkType { get; set; }
        public string AddressType { get; set; }
        public string UnicastAddress { get; set; }
    }

}
