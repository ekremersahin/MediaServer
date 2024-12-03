using MediaServer.Media.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Media.Models
{
    //STEP2
    public class VideoConstraints
    {
        // Çözünürlük
        public Resolution Resolution { get; set; }

        // Kare hızı
        public FrameRate FrameRate { get; set; }

        // Bant genişliği sınırları
        public BandwidthLimit Bandwidth { get; set; }

        // Codec tercihleri
        public List<CodecInfo> VideoCodecs { get; set; } = new List<CodecInfo>();

        // Kamera/ekran paylaşımı ayarları
        public VideoSourceType SourceType { get; set; } = VideoSourceType.Camera;
    }
}
