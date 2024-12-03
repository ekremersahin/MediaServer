using System;

namespace MediaServer.RTC.Models
{
    public class MediaStream
    {
        public string Id { get; }
        public bool HasVideo { get; set; }
        public bool HasAudio { get; set; }
        public MediaStreamTrack[] Tracks { get; set; }

        public MediaStream()
        {
            Id = Guid.NewGuid().ToString();
            Tracks = Array.Empty<MediaStreamTrack>();
        }
    }
}