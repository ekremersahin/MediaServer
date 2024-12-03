
using MediaServer.SDP.Interfaces;
using MediaServer.SDP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.SDP.Services
{

    public class SDPValidator : ISDPValidator
    {
        public ValidationResult Validate(SessionDescription session)
        {
            var result = new ValidationResult();

            ValidateVersion(session, result);
            ValidateOrigin(session, result);
            ValidateSessionName(session, result);
            ValidateConnection(session, result);
            ValidateTime(session, result);
            ValidateMedia(session, result);
            ValidateBandwidth(session, result);
            ValidateSecurityAttributes(session, result);
            return result;
        }

        private void ValidateVersion(SessionDescription session, ValidationResult result)
        {
            if (string.IsNullOrEmpty(session.Version))
                result.Errors.Add(new ValidationError { Field = "Version", Message = "Version is required" });
            else if (session.Version != "0")
                result.Errors.Add(new ValidationError { Field = "Version", Message = "Version must be 0" });
        }

        private void ValidateOrigin(SessionDescription session, ValidationResult result)
        {
            if (session.Origin == null)
            {
                result.Errors.Add(new ValidationError { Field = "Origin", Message = "Origin is required" });
                return;
            }

            if (string.IsNullOrEmpty(session.Origin.Username))
                result.Errors.Add(new ValidationError { Field = "Origin.Username", Message = "Username is required" });

            if (string.IsNullOrEmpty(session.Origin.SessionId))
                result.Errors.Add(new ValidationError { Field = "Origin.SessionId", Message = "SessionId is required" });

            if (string.IsNullOrEmpty(session.Origin.NetworkType))
                result.Errors.Add(new ValidationError { Field = "Origin.NetworkType", Message = "NetworkType is required" });

            if (string.IsNullOrEmpty(session.Origin.AddressType))
                result.Errors.Add(new ValidationError { Field = "Origin.AddressType", Message = "AddressType is required" });

            if (string.IsNullOrEmpty(session.Origin.UnicastAddress))
                result.Errors.Add(new ValidationError { Field = "Origin.UnicastAddress", Message = "UnicastAddress is required" });
        }

        private void ValidateSessionName(SessionDescription session, ValidationResult result)
        {
            if (string.IsNullOrEmpty(session.SessionName))
                result.Errors.Add(new ValidationError { Field = "SessionName", Message = "Session name is required" });
        }

        private void ValidateConnection(SessionDescription session, ValidationResult result)
        {
            if (session.Connection == null)
            {
                result.Errors.Add(new ValidationError { Field = "Connection", Message = "Connection is required" });
                return;
            }

            if (string.IsNullOrEmpty(session.Connection.NetworkType))
                result.Errors.Add(new ValidationError { Field = "Connection.NetworkType", Message = "NetworkType is required" });

            if (string.IsNullOrEmpty(session.Connection.AddressType))
                result.Errors.Add(new ValidationError { Field = "Connection.AddressType", Message = "AddressType is required" });

            if (string.IsNullOrEmpty(session.Connection.Address))
                result.Errors.Add(new ValidationError { Field = "Connection.Address", Message = "Address is required" });
        }

        private void ValidateTime(SessionDescription session, ValidationResult result)
        {
            if (session.Time == null)
                result.Errors.Add(new ValidationError { Field = "Time", Message = "Time description is required" });
        }

        private void ValidateMedia(SessionDescription session, ValidationResult result)
        {
            if (session.Media == null || !session.Media.Any())
            {
                result.Errors.Add(new ValidationError { Field = "Media", Message = "At least one media description is required" });
                return;
            }

            foreach (var media in session.Media)
            {
                if (string.IsNullOrEmpty(media.Type))
                    result.Errors.Add(new ValidationError { Field = "Media.Type", Message = "Media type is required" });

                if (media.Port <= 0 || media.Port > 65535)
                    result.Errors.Add(new ValidationError { Field = "Media.Port", Message = "Invalid port number" });

                if (string.IsNullOrEmpty(media.Protocol))
                    result.Errors.Add(new ValidationError { Field = "Media.Protocol", Message = "Protocol is required" });

                if (media.FormatIds == null || !media.FormatIds.Any())
                    result.Errors.Add(new ValidationError { Field = "Media.FormatIds", Message = "At least one format ID is required" });
            }
        }

        private void ValidateBandwidth(SessionDescription session, ValidationResult result)
        {
            // Bandwidth kısıtlamaları kontrolü
            if (session.Attributes.TryGetValue("b", out var bandwidth))
            {
                if (!int.TryParse(bandwidth, out var bandwidthValue) || bandwidthValue <= 0)
                {
                    result.Errors.Add(new ValidationError
                    {
                        Field = "Bandwidth",
                        Message = "Invalid bandwidth specification"
                    });
                }
            }
        }
        private void ValidateSecurityAttributes(SessionDescription session, ValidationResult result)
        {
            // Crypto attribute kontrolü
            bool hasCrypto = session.Media.Any(m =>
                m.Attributes.ContainsKey("crypto") ||
                m.Attributes.ContainsKey("fingerprint"));

            if (!hasCrypto)
            {
                result.Errors.Add(new ValidationError
                {
                    Field = "Security",
                    Message = "No security attributes found"
                });
            }
        }
    }
}
