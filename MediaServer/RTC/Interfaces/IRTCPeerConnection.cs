using MediaServer.RTC.Models;
using System;
using System.Threading.Tasks;

namespace MediaServer.RTC.Interfaces
{
    public interface IRTCPeerConnection
    {
        RTCPeerConnectionState ConnectionState { get; }
        event EventHandler<MediaStream> OnTrack;
        event EventHandler<RTCPeerConnectionState> OnConnectionStateChange;

        Task<string> CreateOfferAsync();
        Task<string> CreateAnswerAsync();
        Task SetLocalDescriptionAsync(string sdp);
        Task SetRemoteDescriptionAsync(string sdp);
        Task AddIceCandidateAsync(string candidate);
        Task AddTrackAsync(MediaStreamTrack track);
        Task Close();
    }
}