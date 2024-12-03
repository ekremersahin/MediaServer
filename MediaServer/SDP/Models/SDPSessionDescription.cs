namespace MediaServer.SDP.Models
{
    public class SDPSessionDescription
    {
        public string Type { get; set; }
        public string Sdp { get; set; }

        public SDPSessionDescription(string type, string sdp)
        {
            Type = type;
            Sdp = sdp;
        }
    }
}