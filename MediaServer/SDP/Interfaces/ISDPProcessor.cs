using MediaServer.ICE.Models;
using MediaServer.Media.Models;
using MediaServer.SDP.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.SDP.Interfaces
{
    public interface ISDPProcessor
    {
        Task<ProcessResult> ProcessSessionAsync(SessionDescription session);
        Task<MediaProcessResult> ProcessMediaAsync(MediaDescription media);
        Task<bool> ValidateAndInitializeAsync(SessionDescription session);
        Task<ConnectionStatus> EstablishConnectionAsync(ConnectionInfo connection);


        Task<SDPSessionDescription> CreateOfferAsync(IEnumerable<ICECandidate> candidates);
        Task<SDPSessionDescription> CreateAnswerAsync(IEnumerable<ICECandidate> candidates, SDPSessionDescription remoteDescription);

        Task<SDPSessionDescription> AddIceCandidateAsync(ICECandidate candidate, SDPSessionDescription localDescription);

        //Task SetLocalDescriptionAsync(SDPSessionDescription description);
        //Task SetRemoteDescriptionAsync(SDPSessionDescription description);

    }
}
