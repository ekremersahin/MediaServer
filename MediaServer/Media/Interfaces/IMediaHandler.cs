using MediaServer.Media.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Media.Interfaces
{
    public interface IMediaHandler
    {
        Task<Dictionary<string, string>> ExtractParametersAsync(MediaDescription media);
        Task<List<string>> GetSupportedCodecsAsync(MediaDescription media);
        Task<bool> InitializeMediaAsync(MediaDescription media);
        Task<string> ParseCodecAsync(string codecString);
        Task<Dictionary<string, string>> ParseFormatParametersAsync(string fmtpString);
        Task<(string Suite, string Key)> ParseCryptoParametersAsync(string cryptoString);



        // Yeni metodlar
        //STEP3
        Task<MediaProcessingResult> ProcessMediaAsync(MediaDescription media, Stream inputStream, Stream outputStream);
        Task<CodecInfo> GetPreferredCodecAsync(MediaDescription media);

    }

}
