using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.SignalizationServer.Models
{

    public enum PeerStatus
    {
        Offline,
        Online,
        Busy,
        Away
    }

    public enum MediaType
    {
        Audio,
        Video,
        Screen,
        DataChannel
    }

}
