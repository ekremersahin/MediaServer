using MediaServer.Media.Interfaces;
using MediaServer.Media.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Media.Services
{
    public class MediaHandlerFactory : IMediaHandlerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public MediaHandlerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IMediaHandler CreateHandler(string mediaType)
        {
            return mediaType.ToLower() switch
            {
                "audio" => _serviceProvider.GetRequiredService<AudioMediaHandler>(),
                "video" => _serviceProvider.GetRequiredService<VideoMediaHandler>(),
                _ => throw new NotSupportedException($"Media type {mediaType} not supported")
            };
        }

        //STEP2
        public IMediaHandler CreateHandler(MediaStreamConstraints constraints)
        {
            return constraints.Video != null
                ? _serviceProvider.GetRequiredService<VideoMediaHandler>()
                : _serviceProvider.GetRequiredService<AudioMediaHandler>();
        }

    }
}
