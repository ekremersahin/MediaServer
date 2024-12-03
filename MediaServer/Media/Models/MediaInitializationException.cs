using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Media.Models
{
    public class MediaInitializationException : Exception
    {
        public MediaInitializationException(string message) : base(message) { }
    }
}
