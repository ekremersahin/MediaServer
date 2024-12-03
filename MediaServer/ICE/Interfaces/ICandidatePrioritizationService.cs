using MediaServer.ICE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.ICE.Interfaces
{
    public interface ICandidatePrioritizationService
    {
        Task<IEnumerable<ICECandidate>> PrioritizeCandidatesAsync(
            IEnumerable<ICECandidate> candidates,
            CancellationToken cancellationToken = default);
    }
}
