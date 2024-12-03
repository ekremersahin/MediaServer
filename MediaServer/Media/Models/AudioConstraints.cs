using MediaServer.Media.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Media.Models
{
    //STEP2
    public class AudioConstraints
    {
        // Ses özellikleri
        public AudioQualityProfile QualityProfile { get; set; } = AudioQualityProfile.Default;

        // Codec tercihleri
        public List<CodecInfo> AudioCodecs { get; set; } = new List<CodecInfo>();

        // Ses kaynağı
        public AudioSourceType SourceType { get; set; } = AudioSourceType.Microphone;

        // Gelişmiş ses işleme
        public AudioProcessingOptions ProcessingOptions { get; set; } = new AudioProcessingOptions();
    }
}
