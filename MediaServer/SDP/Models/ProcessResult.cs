using MediaServer.Media.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.SDP.Models
{
    public class ProcessResult
    {
        public bool Success { get; set; }
        public string SessionId { get; set; }
        public List<MediaProcessResult> MediaResults { get; set; } = new List<MediaProcessResult>();
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public List<string> Errors { get; set; } = new List<string>();
    }

}
