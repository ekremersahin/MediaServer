namespace MediaServer.RTC.Models
{
    public class MediaStreamTrack
    {
        public string Id { get; set; }
        public string Kind { get; set; } // "audio" or "video"
        public bool Enabled { get; set; }
        public TrackState State { get; set; }
    }

    public enum TrackState
    {
        Live,
        Ended
    }
}