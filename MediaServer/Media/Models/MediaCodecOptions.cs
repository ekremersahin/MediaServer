using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Media.Models
{
    public class MediaCodecOptions
    {
        public string Name { get; set; }
        public int? Bitrate { get; set; }
        public Resolution Resolution { get; set; }
        public FrameRate FrameRate { get; set; }
    }
}
