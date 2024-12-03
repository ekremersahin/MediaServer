using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.ICE.Models
{
    public class NetworkInterface
    {
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public bool IsActive { get; set; }
        public NetworkInterfaceType Type { get; set; }
    }

}
