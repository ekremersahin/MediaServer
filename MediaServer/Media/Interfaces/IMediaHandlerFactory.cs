using MediaServer.Media.Models;
using MediaServer.Media.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Media.Interfaces
{
    public interface IMediaHandlerFactory
    {
        IMediaHandler CreateHandler(string mediaType);


        //STEP2
        IMediaHandler CreateHandler(MediaStreamConstraints constraints);
    }
}
