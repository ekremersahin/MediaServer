using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Media.Models
{
    public class MediaProcessingResult
    {
        public bool Success { get; set; }
        public string MediaType { get; set; }
        public List<string> Codecs { get; set; }
        public Dictionary<string, string> ProcessedParameters { get; set; }
        public List<string> Warnings { get; set; }
        public List<string> Errors { get; set; }
    }

}
