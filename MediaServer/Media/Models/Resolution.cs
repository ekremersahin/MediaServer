using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Media.Models
{
    //STEP2
    public class Resolution
    {
        public int Width { get; set; }
        public int Height { get; set; }



        // Standart çözünürlükler
        public static Resolution HD = new Resolution { Width = 1280, Height = 720 };
        public static Resolution FullHD = new Resolution { Width = 1920, Height = 1080 };
        public static Resolution UHD = new Resolution { Width = 3840, Height = 2160 };


        public Resolution(int width = 1920, int height = 1080)
        {
            Width = width;
            Height = height;
        }

        public static Resolution FromMediaDescription(MediaDescription media)
        {
            // MediaDescription'dan çözünürlük çıkarma
            if (media.Width.HasValue && media.Height.HasValue)
            {
                return new Resolution(media.Width.Value, media.Height.Value);
            }

            // Varsayılan çözünürlükler
            return media.Type switch
            {
                "Video" => new Resolution(1280, 720), // HD
                "Screen" => new Resolution(1920, 1080), // Full HD
                _ => new Resolution(640, 480) // Varsayılan
            };
        }

    }
}
