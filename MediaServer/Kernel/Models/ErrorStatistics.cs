using MediaServer.Kernel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Kernel.Models
{
    // Hata istatistikleri modeli
    public class ErrorStatistics
    {
        public int TotalErrorCount { get; set; }
        public Dictionary<ErrorSeverity, int> ErrorsBySeverity { get; set; }
        public Dictionary<string, int> ErrorsByType { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

}
