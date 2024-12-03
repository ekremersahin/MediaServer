using MediaServer.Media.Interfaces;
using MediaServer.Media.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaServer.Media.Services
{
    public class AudioMediaHandler : IMediaHandler
    {
        public async Task<Dictionary<string, string>> ExtractParametersAsync(MediaDescription media)
        {
            if (media == null)
            {
                throw new ArgumentNullException(nameof(media));
            }

            var parameters = new Dictionary<string, string>();

            // Temel medya özelliklerini ekleyelim
            parameters["Type"] = media.Type;
            parameters["Port"] = media.Port.ToString();
            parameters["Protocol"] = media.Protocol;

            // Medya özniteliklerini işleyelim
            if (media.Attributes != null && media.Attributes.Any())
            {
                foreach (var attribute in media.Attributes)
                {
                    if (attribute.Key.StartsWith("rtpmap"))
                    {
                        var codecInfo = await ParseCodecAsync(attribute.Value);
                        parameters["Codec"] = codecInfo;
                    }
                    else if (attribute.Key.StartsWith("fmtp"))
                    {
                        var formatParameters = await ParseFormatParametersAsync(attribute.Value);
                        foreach (var param in formatParameters)
                        {
                            parameters[param.Key] = param.Value;
                        }
                    }
                    else if (attribute.Key.StartsWith("crypto"))
                    {
                        var cryptoParams = await ParseCryptoParametersAsync(attribute.Value);
                        parameters["CryptoSuite"] = cryptoParams.Suite;
                        parameters["CryptoKey"] = cryptoParams.Key;
                    }
                    else
                    {
                        // Diğer öznitelikleri ekleyelim
                        parameters[attribute.Key] = attribute.Value;
                    }
                }
            }

            return parameters;
        }

        public async Task<string> ParseCodecAsync(string codecString)
        {
            if (string.IsNullOrWhiteSpace(codecString))
            {
                throw new ArgumentException("Codec bilgisi boş olamaz.", nameof(codecString));
            }

            // Örnek codecString: "0 PCMU/8000"
            var parts = codecString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 2)
            {
                var codecFull = parts[1];
                var codecParts = codecFull.Split('/', StringSplitOptions.RemoveEmptyEntries);
                return await Task.FromResult(codecParts[0]); // "PCMU"
            }

            throw new FormatException("Codec bilgisi beklenen formatta değil.");
        }

        public async Task<Dictionary<string, string>> ParseFormatParametersAsync(string fmtpString)
        {
            if (string.IsNullOrWhiteSpace(fmtpString))
            {
                throw new ArgumentException("Format parametreleri boş olamaz.", nameof(fmtpString));
            }

            // Örnek fmtpString: "96 apt=98"
            var parameters = new Dictionary<string, string>();
            var parts = fmtpString.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 2)
            {
                var paramPairs = parts[1].Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var pair in paramPairs)
                {
                    var keyValue = pair.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);
                    if (keyValue.Length == 2)
                    {
                        var key = keyValue[0].Trim();
                        var value = keyValue[1].Trim();
                        parameters[key] = value;
                    }
                }
                return await Task.FromResult(parameters);
            }

            throw new FormatException("Format parametreleri beklenen formatta değil.");
        }

        public async Task<(string Suite, string Key)> ParseCryptoParametersAsync(string cryptoString)
        {
            if (string.IsNullOrWhiteSpace(cryptoString))
            {
                throw new ArgumentException("Kripto parametreleri boş olamaz.", nameof(cryptoString));
            }

            // Örnek cryptoString: "1 AES_CM_128_HMAC_SHA1_80 inline:ke4w...=="
            var parts = cryptoString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 3)
            {
                var suite = parts[1];
                var keyInfo = parts[2];

                if (keyInfo.StartsWith("inline:", StringComparison.OrdinalIgnoreCase))
                {
                    var key = keyInfo.Substring("inline:".Length);
                    return await Task.FromResult((suite, key));
                }
            }

            throw new FormatException("Kripto parametreleri beklenen formatta değil.");
        }

        public async Task<List<string>> GetSupportedCodecsAsync(MediaDescription media)
        {
            // Desteklenen ses codec'leri
            return await Task.FromResult(new List<string> { "PCMU", "PCMA", "Opus", "G722" });
        }

        public async Task<bool> InitializeMediaAsync(MediaDescription media)
        {
            if (media == null || string.IsNullOrEmpty(media.Type))
            {
                return await Task.FromResult(false);
            }
            // Gerekirse ek başlatma işlemleri
            return await Task.FromResult(true);
        }

        public async Task<MediaProcessingResult> ProcessMediaAsync(MediaDescription media, Stream inputStream, Stream outputStream)
        {
            try
            {
                // Ses işleme mantığı burada uygulanmalı
                // Örnek olarak, giriş akışını çıkış akışına kopyalıyoruz
                await inputStream.CopyToAsync(outputStream);

                return new MediaProcessingResult
                {
                    Success = true,
                    MediaType = media.Type,
                    Codecs = new List<string> { },
                    ProcessedParameters = await ExtractParametersAsync(media)
                };
            }
            catch (Exception ex)
            {
                return new MediaProcessingResult
                {
                    Success = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<CodecInfo> GetPreferredCodecAsync(MediaDescription media)
        {
            var supportedCodecs = await GetSupportedCodecsAsync(media);
            return new CodecInfo { Name = supportedCodecs.FirstOrDefault() ?? "Opus" };
        }
    }
}