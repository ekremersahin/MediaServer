using MediaServer.SDP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.SDP.Interfaces
{
    public interface ISDPHandler
    {
        Task<SDPMessage> ProcessOfferAsync(SDPMessage offer);
        Task<SDPMessage> ProcessAnswerAsync(SDPMessage answer);
        Task<SDPMessage> ProcessCandidateAsync(SDPMessage candidate);
    }
}
