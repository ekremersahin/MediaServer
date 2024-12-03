using MediaServer.SDP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.SignalizationServer.Interfaces
{
    public interface IWebSocketMediator
    {
        Task HandleMessageAsync(string clientId, SDPMessage message, IWebSocketManager webSocket);
        Task<Boolean> isProccesType(string typename);

    }

}
