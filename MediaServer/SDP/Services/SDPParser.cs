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
    public class SDPParser : ISDPParser
    {
        public SessionDescription Parse(string sdpMessage)
        {
            var session = new SessionDescription
            {
                Media = new List<MediaDescription>(),
                Attributes = new Dictionary<string, string>()
            };

            var lines = sdpMessage.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            MediaDescription currentMedia = null;

            foreach (var line in lines)
            {
                var parts = line.Split('=');
                if (parts.Length != 2) continue;

                var type = parts[0];
                var value = parts[1];

                switch (type)
                {
                    case "v":
                        session.Version = value;
                        break;
                    case "o":
                        session.Origin = ParseOrigin(value);
                        break;
                    case "s":
                        session.SessionName = value;
                        break;
                    case "c":
                        session.Connection = ParseConnection(value);
                        break;
                    case "t":
                        session.Time = ParseTime(value);
                        break;
                    case "m":
                        currentMedia = ParseMedia(value);
                        session.Media.Add(currentMedia);
                        break;
                    case "a":
                        ParseAttribute(value, currentMedia, session);
                        break;
                }
            }

            return session;
        }

        private Origin ParseOrigin(string value)
        {
            var parts = value.Split(' ');
            if (parts.Length != 6)
                throw new ArgumentException("Invalid origin format");

            return new Origin
            {
                Username = parts[0],
                SessionId = parts[1],
                SessionVersion = parts[2],
                NetworkType = parts[3],
                AddressType = parts[4],
                UnicastAddress = parts[5]
            };
        }

        private ConnectionInfo ParseConnection(string value)
        {
            var parts = value.Split(' ');
            if (parts.Length != 3)
                throw new ArgumentException("Invalid connection format");

            return new ConnectionInfo
            {
                NetworkType = parts[0],
                AddressType = parts[1],
                Address = parts[2]
            };
        }

        private TimeDescription ParseTime(string value)
        {
            var parts = value.Split(' ');
            if (parts.Length != 2)
                throw new ArgumentException("Invalid time format");

            return new TimeDescription
            {
                StartTime = long.Parse(parts[0]),
                StopTime = long.Parse(parts[1])
            };
        }

        private MediaDescription ParseMedia(string value)
        {
            var parts = value.Split(' ');
            if (parts.Length < 4)
                throw new ArgumentException("Invalid media format");

            return new MediaDescription
            {
                Type = parts[0],
                Port = int.Parse(parts[1]),
                Protocol = parts[2],
                FormatIds = parts.Skip(3).Select(int.Parse).ToList(),
                Attributes = new Dictionary<string, string>()
            };
        }

        private void ParseAttribute(string value, MediaDescription currentMedia, SessionDescription session)
        {
            var parts = value.Split(':');
            var key = parts[0];
            var attributeValue = parts.Length > 1 ? parts[1] : string.Empty;

            if (currentMedia != null)
            {
                currentMedia.Attributes[key] = attributeValue;
            }
            else
            {
                session.Attributes[key] = attributeValue;
            }
        }



    }
}
