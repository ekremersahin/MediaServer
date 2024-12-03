namespace MediaServer.ICE.Models
{
    public class TURNResponse
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public string PublicIpAddress { get; set; }
        public ushort PublicPort { get; set; }
    }
}