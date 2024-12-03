using MediaServer.Kernel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Kernel.Models
{
    public class TrackingModel //: ITrackingModel
    {


        public string MetricName { get; init; }
        public double Value { get; init; }
        public DateTime Timestamp { get; init; }
        public Dictionary<string, string> Properties { get; init; }
    }
}
