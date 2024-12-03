using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.ICE.Exc
{
    public class StunConnectionException : Exception
    {
        public StunConnectionException(string message, Exception inner = null) : base(message, inner) { }
    }

    public class StunException : Exception
    {
        public StunException(string message, Exception inner = null) : base(message, inner) { }
    }
}
