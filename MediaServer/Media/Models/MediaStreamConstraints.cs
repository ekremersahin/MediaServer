using MediaServer.Media.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Media.Models
{
    //STEP2
    public class MediaStreamConstraints
    {
        // Temel medya akış özellikleri
        public Guid StreamId { get; set; } = Guid.NewGuid();
        public MediaDescription MediaDescription { get; set; }

        // Video kısıtlamaları
        public VideoConstraints Video { get; set; }

        // Ses kısıtlamaları
        public AudioConstraints Audio { get; set; }

        // Codec bilgileri
        public List<CodecInfo> PreferredCodecs { get; set; } = new List<CodecInfo>();

        // Ek özellikler
        public bool IsScreenShare { get; set; }
        public TransportProtocol TransportProtocol { get; set; } = TransportProtocol.Udp;
    }

}
