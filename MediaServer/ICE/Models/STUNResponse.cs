using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.ICE.Models
{
    public class STUNResponse
    {
        public string PublicIpAddress { get; set; }
        public int PublicPort { get; set; }
        public bool IsNatDetected { get; set; }
        public NatType NatType { get; set; }
    }

}
