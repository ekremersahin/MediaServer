using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.ICE.Models
{
    public class StunClientOptions
    {
        public int ConnectTimeout { get; set; } = 5000;
        public int ReceiveTimeout { get; set; } = 5000;
        public int SendTimeout { get; set; } = 5000;
        public int MaxPoolSize { get; set; } = 100;
    }

}
