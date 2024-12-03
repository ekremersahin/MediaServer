using MediaServer.ICE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.ICE.Interfaces
{
    public interface INetworkConditionService
    {
        Task<NetworkCondition> GetCurrentConditionAsync(CancellationToken cancellationToken = default);
        Task<NetworkCondition> TestCandidateNetworkPerformanceAsync(ICECandidate candidate, CancellationToken cancellationToken = default);
    }
}
