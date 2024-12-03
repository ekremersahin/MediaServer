using MediaServer.Media.Interfaces;
using MediaServer.Media.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Media.Services
{
    public class MediaHandler : IMediaHandler
    {
        public async Task<Dictionary<string, string>> ExtractParametersAsync(MediaDescription media)
            => media.Attributes ?? new Dictionary<string, string>();

        public async Task<List<string>> GetSupportedCodecsAsync(MediaDescription media)
            => new List<string> { "PCMU", "PCMA" };

        public async Task<bool> InitializeMediaAsync(MediaDescription media)
            => true;

        public async Task<string> ParseCodecAsync(string codecString)
            => codecString;

        public async Task<Dictionary<string, string>> ParseFormatParametersAsync(string fmtpString)
            => new Dictionary<string, string>();

        public async Task<(string Suite, string Key)> ParseCryptoParametersAsync(string cryptoString)
            => ("AES_CM_128_HMAC_SHA1_80", "inline:key");

        public Task<MediaProcessingResult> ProcessMediaAsync(MediaDescription media, Stream inputStream, Stream outputStream)
        {
            throw new NotImplementedException();
        }

        public Task<CodecInfo> GetPreferredCodecAsync(MediaDescription media)
        {
            throw new NotImplementedException();
        }
    }

}
