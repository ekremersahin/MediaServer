using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.SignalizationServer.Models
{
    public class PeerMetadata
    {
        public string ClientId { get; set; }
        public string Username { get; set; }
        public List<MediaType> SupportedMediaTypes { get; set; } = new List<MediaType>();
        public ConnectionCapabilities Capabilities { get; set; }
        public DateTime LastActiveTimestamp { get; set; }
        public PeerStatus Status { get; set; } = PeerStatus.Offline;
    }
}
