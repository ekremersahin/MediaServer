using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Media.Models
{
    //STEP2
    public class AudioProcessingOptions
    {
        public bool EchoCancellation { get; set; } = true;
        public bool NoiseReduction { get; set; } = true;
        public bool AutoGainControl { get; set; } = true;
    }
}
