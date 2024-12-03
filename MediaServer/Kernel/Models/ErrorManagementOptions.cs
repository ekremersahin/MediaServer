using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Kernel.Models
{
    public class ErrorManagementOptions
    {
        public bool EnableDetailedLogging { get; set; }
        public int MaxRetryAttempts { get; set; }
        public TimeSpan RetryDelay { get; set; }
        public List<string> NotificationChannels { get; set; }
    }
}
