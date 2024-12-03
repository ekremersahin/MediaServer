using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.ICE.Exc
{
    public class StunProtocolException : Exception
    {
        public StunProtocolException(string message) : base(message) { }
    }

}
