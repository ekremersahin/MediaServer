using MediaServer.Kernel;
using MediaServer.Kernel.Interfaces;
using MediaServer.Kernel.Models;
using MediaServer.SDP.Interfaces;
using MediaServer.SDP.Models;
using MediaServer.SignalizationServer;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.SDP.Services
{
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }

    public class ProcessingException : Exception
    {
        public ProcessingException(string message) : base(message) { }
    }

    public class SDPHandler : ISDPHandler
    {
        private readonly ILogger<SDPHandler> _logger;
        private readonly ITelemetryService telemetryService;
        private readonly ISDPParser _sdpParser;
        private readonly ISDPValidator _sdpValidator;
        private readonly ISDPProcessor _sdpProcessor;
        private readonly ISDPGenerator _sdpGenerator;

        public SDPHandler(
            ILogger<SDPHandler> logger,
            ITelemetryService telemetryService,
            // IWebSocketManager webSocketManager,
            ISDPParser sdpParser,
            ISDPValidator sdpValidator,
            ISDPProcessor sdpProcessor,
            ISDPGenerator sdpGenerator) // Constructor'a ekledik
        {
            _logger = logger;
            this.telemetryService = telemetryService;
            //_webSocketManager = webSocketManager;
            _sdpParser = sdpParser;
            _sdpValidator = sdpValidator;
            _sdpProcessor = sdpProcessor;
            _sdpGenerator = sdpGenerator; // Atama 


        }


        public async Task<SDPMessage> ProcessOfferAsync(SDPMessage offerMessage)
        {
            try
            {
                // SDP'yi parse et
                var sessionDescription = _sdpParser.Parse(offerMessage.Sdp);

                // Validasyon
                var validationResult = _sdpValidator.Validate(sessionDescription);
                if (!validationResult.IsValid)
                {

                    _ = telemetryService.TrackErrorAsync(new TrackingModel()
                    {
                        Timestamp = DateTime.UtcNow,
                        MetricName = "SDPValidationFailed",
                        Value = 0,
                        Properties = new Dictionary<string, string> {
                            { "SDP", offerMessage.Sdp },
                            { "Errors", string.Join(", ", validationResult.Errors.Select(e => e.Message)) }
                        }
                    }, null);
                    return null;
                    //throw new ValidationException("SDP Validation Failed");
                }

                // İşleme
                var processResult = await _sdpProcessor.ProcessSessionAsync(sessionDescription);

                if (!processResult.Success)
                {


                    _ = telemetryService.TrackErrorAsync(new TrackingModel()
                    {
                        Timestamp = DateTime.UtcNow,
                        MetricName = "SDPProcessingFailed",
                        Value = 0,
                        Properties = new Dictionary<string, string> {
                            { "SDP", offerMessage.Sdp },
                            { "Errors", string.Join(", ", processResult.Errors) }
                        }
                    }, null);

                    return null;
                }

                _ = telemetryService.TrackMetricAsync(new TrackingModel()
                {
                    Timestamp = DateTime.UtcNow,
                    MetricName = "SDPProcessed",
                    Value = 1,
                    Properties = new Dictionary<string, string> {
                        { "SDP", offerMessage.Sdp }
                    }
                });

                _ = telemetryService.TrackErrorAsync(new TrackingModel()
                {
                    Timestamp = DateTime.UtcNow,
                    MetricName = "SDPProcessingFailed",
                    Value = 0,
                    Properties = new Dictionary<string, string> {
                            { "SDP", offerMessage.Sdp },
                            { "Errors", string.Join(", ", processResult.Errors) }
                        }
                }, null);


                return offerMessage;
            }
            catch (Exception ex)
            {

                _ = telemetryService.TrackErrorAsync(new TrackingModel()
                {
                    Timestamp = DateTime.UtcNow,
                    MetricName = "SDPProcessingFailed",
                    Value = 0,
                    Properties = new Dictionary<string, string> {
                        { "SDP", offerMessage.Sdp },
                        { "Errors", ex.Message }
                    }
                }, null);

                return null;
                //throw;
            }
        }
        public async Task<SDPMessage> ProcessAnswerAsync(SDPMessage answerMessage)
        {
            // Benzer işlemler offer ile aynı
            try
            {
                var sessionDescription = _sdpParser.Parse(answerMessage.Sdp);
                var validationResult = _sdpValidator.Validate(sessionDescription);

                if (!validationResult.IsValid)
                {

                    _ = telemetryService.TrackErrorAsync(new TrackingModel()
                    {
                        Timestamp = DateTime.UtcNow,
                        MetricName = "SDPValidationFailed",
                        Value = 0,
                        Properties = new Dictionary<string, string> {
                            { "SDP", answerMessage.Sdp },
                            { "Errors", string.Join(", ", validationResult.Errors.Select(e => e.Message)) }
                        }
                    }, null);

                    return null;
                    //throw new ValidationException("SDP Answer Validation Failed");
                }

                var processResult = await _sdpProcessor.ProcessSessionAsync(sessionDescription);

                if (!processResult.Success)
                {

                    _ = telemetryService.TrackErrorAsync(new TrackingModel()
                    {
                        Timestamp = DateTime.UtcNow,
                        MetricName = "SDPProcessingFailed",
                        Value = 0,
                        Properties = new Dictionary<string, string> {
                            { "SDP", answerMessage.Sdp },
                            { "Errors", string.Join(", ", processResult.Errors) }
                        }
                    }, null);
                    return null;
                    //throw new ProcessingException("SDP Answer Processing Failed");
                }

                _ = telemetryService.TrackMetricAsync(new TrackingModel()
                {
                    Timestamp = DateTime.UtcNow,
                    MetricName = "SDPProcessed",
                    Value = 1,
                    Properties = new Dictionary<string, string> {
                        { "SDP", answerMessage.Sdp }
                    }
                });
                return answerMessage;
            }
            catch (Exception ex)
            {

                _ = telemetryService.TrackErrorAsync(new TrackingModel()
                {
                    Timestamp = DateTime.UtcNow,
                    MetricName = "SDPProcessingFailed",
                    Value = 0,
                    Properties = new Dictionary<string, string> {
                        { "SDP", answerMessage.Sdp },
                        { "Errors", ex.Message }
                    }
                }, null);

                return null;
                //throw;
            }
        }

        public async Task<SDPMessage> ProcessCandidateAsync(SDPMessage candidateMessage)
        {
            // ICE candidate işleme
            // Gelecekte ICE candidate için özel bir işlem eklenebilir
            _logger.LogInformation($"Processing ICE Candidate: {candidateMessage.Sdp}");
            // signalEvents.OnMyEvent(new MyEventArgs("offerMessage.Sdp"));
            //await signalContext.BroadcastMessageAsync(candidateMessage.Sdp);

            return candidateMessage;
        }

        public string CreateOffer(SessionDescription sessionDescription)
        {
            // Validasyon
            var validationResult = _sdpValidator.Validate(sessionDescription);
            if (!validationResult.IsValid)
            {

                _ = telemetryService.TrackErrorAsync(new TrackingModel()
                {
                    Timestamp = DateTime.UtcNow,
                    MetricName = "SDPValidationFailed",
                    Value = 0,
                    Properties = new Dictionary<string, string> {
                        { "SDP", sessionDescription.ToString() },
                        { "Errors", string.Join(", ", validationResult.Errors.Select(e => e.Message)) }
                    }
                }, null);

                return null;
                //throw new ValidationException("SDP Offer Validation Failed");
            }

            // SDP string'i oluştur

            _ = telemetryService.TrackMetricAsync(new TrackingModel()
            {
                Timestamp = DateTime.UtcNow,
                MetricName = "SDPOfferCreated",
                Value = 1,
                Properties = new Dictionary<string, string> {
                    { "SDP", sessionDescription.ToString() }
                }
            });
            return _sdpGenerator.Generate(sessionDescription);
        }

        public string CreateAnswer(SessionDescription sessionDescription)
        {
            // Validasyon
            var validationResult = _sdpValidator.Validate(sessionDescription);
            if (!validationResult.IsValid)
            {
                _ = telemetryService.TrackErrorAsync(new TrackingModel()
                {
                    Timestamp = DateTime.UtcNow,
                    MetricName = "SDPValidationFailed",
                    Value = 0,
                    Properties = new Dictionary<string, string> {
                        { "SDP", sessionDescription.ToString() },
                        { "Errors", string.Join(", ", validationResult.Errors.Select(e => e.Message)) }
                    }
                }, null);
                return null;
                //throw new ValidationException("SDP Answer Validation Failed");
            }

            // SDP string'i oluştur
            _ = telemetryService.TrackMetricAsync(new TrackingModel()
            {
                Timestamp = DateTime.UtcNow,
                MetricName = "SDPAnswerCreated",
                Value = 1,
                Properties = new Dictionary<string, string> {
                    { "SDP", sessionDescription.ToString() }
                }
            });
            return _sdpGenerator.Generate(sessionDescription);
        }
    }

}
