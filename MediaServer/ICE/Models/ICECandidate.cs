using MediaServer.ICE.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.ICE.Models
{
    public class ICECandidate
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Foundation { get; set; }
        public int Priority { get; set; }
        public string Protocol { get; set; }
        public string Type { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public string CandidateType { get; set; }
        public string TransportType { get; set; }
        public string RelatedAddress { get; set; }
        public int RelatedPort { get; set; }
        public string TcpType { get; set; }
        public int ComponentId { get; set; }


        public CandidateType CandidateTypeEnum
        {
            get => ConvertToCandidateType(Type);
            set => Type = ConvertToTypeString(value);
        }

        private static CandidateType ConvertToCandidateType(string typeString)
        {
            return typeString?.ToLowerInvariant() switch
            {
                "host" => Interfaces.CandidateType.Host,
                "srflx" => Interfaces.CandidateType.ServerReflexive,
                "prflx" => Interfaces.CandidateType.PeerReflexive,
                "relay" => Interfaces.CandidateType.Relayed,
                _ => Interfaces.CandidateType.Unknown
            };
        }

        private static string ConvertToTypeString(CandidateType type)
        {
            return type switch
            {
                Interfaces.CandidateType.Host => "host",
                Interfaces.CandidateType.ServerReflexive => "srflx",
                Interfaces.CandidateType.PeerReflexive => "prflx",
                Interfaces.CandidateType.Relayed => "relay",
                _ => "unknown"
            };
        }

    }

}
