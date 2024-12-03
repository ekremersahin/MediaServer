using MediaServer.RTC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Media.Interfaces
{
    public interface IMediaRouter
    {
        Task RegisterClientAsync(string clientId, MediaStream stream);
        Task RouteMediaAsync(string sourceClientId, MediaStream stream);
        Task UnregisterClientAsync(string clientId);
    }
}
