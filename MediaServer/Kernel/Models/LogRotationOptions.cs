using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Kernel.Models
{
    public class LogRotationOptions
    {
        public string LogDirectory { get; set; }
        public TimeSpan MaxLogAge { get; set; } = TimeSpan.FromDays(30);
        public long MaxTotalLogSizeMB { get; set; } = 500; // Varsayılan 500 MB
    }
}
