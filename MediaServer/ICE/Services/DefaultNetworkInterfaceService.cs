using MediaServer.ICE.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.ICE.Services
{
    public class DefaultNetworkInterfaceService : INetworkInterfaceService
    {
        public async Task<IEnumerable<MediaServer.ICE.Models.NetworkInterface>> GetLocalNetworkInterfacesAsync()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Select(n => new MediaServer.ICE.Models.NetworkInterface
                {
                    Name = n.Name,
                    IpAddress = n.GetIPProperties().UnicastAddresses
                        .FirstOrDefault(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        ?.Address.ToString(),
                    MacAddress = string.Join(":", n.GetPhysicalAddress().GetAddressBytes().Select(b => b.ToString("X2"))),
                    IsActive = true,
                    Type = n.NetworkInterfaceType
                })
                .Where(ni => !string.IsNullOrEmpty(ni.IpAddress))
                .ToList();

            return interfaces;
        }
    }

}
