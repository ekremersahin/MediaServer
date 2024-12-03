using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Media.Models
{
    //STEP2
    public class FrameRate
    {
        public int Fps { get; set; }

        // Standart kare hızları
        public static FrameRate Low = new FrameRate { Fps = 15 };
        public static FrameRate Standard = new FrameRate { Fps = 30 };
        public static FrameRate High = new FrameRate { Fps = 60 };

        public FrameRate(int fps = 30)
        {
            Fps = fps;
        }

        public static FrameRate FromMediaDescription(MediaDescription media)
        {
            // MediaDescription'dan frame rate çıkarma
            // Eğer medya description'ında frame rate bilgisi varsa onu kullan
            // Yoksa varsayılan değerler ata


            return media.Type switch
            {
                "Video" => new FrameRate(30), // Standart video frame rate
                "Screen" => new FrameRate(60), // Ekran paylaşımı için yüksek frame rate
                "Audio" => new FrameRate(0), // Ses için frame rate uygulanmaz
                _ => new FrameRate(24) // Varsayılan
            };
        }

    }
}
