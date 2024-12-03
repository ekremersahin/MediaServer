using MediaServer.Media.Models;
using MediaServer.SDP.Interfaces;
using MediaServer.SDP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.SDP.Services
{
    internal class SDPGenerator : ISDPGenerator
    {
        public string Generate(SessionDescription session)
        {
            var builder = new StringBuilder();

            builder.AppendLine($"v={session.Version}");
            builder.AppendLine($"o={GenerateOrigin(session.Origin)}");
            builder.AppendLine($"s={session.SessionName}");
            builder.AppendLine($"c={GenerateConnection(session.Connection)}");
            builder.AppendLine($"t={GenerateTime(session.Time)}");

            foreach (var attr in session.Attributes)
            {
                builder.AppendLine($"a={attr.Key}:{attr.Value}");
            }

            foreach (var media in session.Media)
            {
                builder.AppendLine(GenerateMedia(media));
                foreach (var attr in media.Attributes)
                {
                    builder.AppendLine($"a={attr.Key}:{attr.Value}");
                }
            }

            return builder.ToString();
        }

        private string GenerateOrigin(Origin origin)
        {
            return $"{origin.Username} {origin.SessionId} {origin.SessionVersion} " +
                   $"{origin.NetworkType} {origin.AddressType} {origin.UnicastAddress}";
        }

        private string GenerateConnection(ConnectionInfo connection)
        {
            return $"{connection.NetworkType} {connection.AddressType} {connection.Address}";
        }

        private string GenerateTime(TimeDescription time)
        {
            return $"{time.StartTime} {time.StopTime}";
        }

        private string GenerateMedia(MediaDescription media)
        {
            return $"m={media.Type} {media.Port} {media.Protocol} {string.Join(" ", media.FormatIds)}";
        }

    }
}
