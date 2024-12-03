
using MediaServer.SDP.Interfaces;
using System.Net.WebSockets;

namespace MediaServer.SignalizationServer
{
    public class SDPManager
    {
        private readonly ISDPProcessor sDPProcessor;
        private readonly ISDPParser sDPParser;
        private readonly ISDPValidator sDPValidator;

        public SDPManager(
            ISDPProcessor sDPProcessor,
            ISDPParser sDPParser,
            ISDPValidator sDPValidator)
        {
            this.sDPProcessor = sDPProcessor;
            this.sDPParser = sDPParser;
            this.sDPValidator = sDPValidator;
        }
        public async void ProcessSDPMessage(string message, WebSocket webSocket)
        {

            var sessionDescription = sDPParser.Parse(message);
            var validationResult = sDPValidator.Validate(sessionDescription);
            var sdpModel = sDPParser.Parse(message);
            var processResult = await sDPProcessor.ProcessSessionAsync(sdpModel);

            if (processResult.Success)
            {
                Console.WriteLine($"Session processed successfully. Session ID: {processResult.SessionId}");
                foreach (var mediaResult in processResult.MediaResults)
                {
                    Console.WriteLine($"Media type: {mediaResult.MediaType}");
                    Console.WriteLine($"Port: {mediaResult.Port}");
                    Console.WriteLine($"Protocol: {mediaResult.Protocol}");
                    Console.WriteLine("Codecs: " + string.Join(", ", mediaResult.Codecs));
                }
            }
            else
            {
                Console.WriteLine("Processing failed:");
                foreach (var error in processResult.Errors)
                {
                    Console.WriteLine($"Error: {error}");
                }
            }
        }
    }
}