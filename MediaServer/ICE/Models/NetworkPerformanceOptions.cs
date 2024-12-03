using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.ICE.Models
{
    public class NetworkPerformanceOptions
    {
        public int PingTimeout { get; set; } = 1000;
        public int MaxConcurrentTests { get; set; } = 5;
        public int CacheDurationMinutes { get; set; } = 15;
        public List<string> SpeedTestServers { get; set; } = new List<string>
        {
            "https://speed.cloudflare.com/__down",
            "https://speed.hetzner.de/__down",
            "https://speedtest.wdc01.softlayer.com/__down"
        };
    }
}
