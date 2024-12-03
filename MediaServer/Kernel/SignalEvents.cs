using MediaServer.SignalizationServer;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Kernel
{
    public class SignalEventSettings
    {
        //private readonly IWebSocketManager socket;
        public SignalEventSettings()
        {
            MyEvent += (sender, e) => Console.WriteLine(e.Message);
        }
        public event EventHandler<MyEventArgs> MyEvent;

        internal void RaiseMyEvent(object sender, MyEventArgs e)
        {
            MyEvent?.Invoke(sender, e);
        }
    }
    public class SignalEvents
    {
        private readonly SignalEventSettings events;


        public SignalEvents(SignalEventSettings events)
        {
            this.events = events;
        }

        protected internal virtual void OnMyEvent(MyEventArgs e)
        {
            events.RaiseMyEvent(this, e);
        }
    }

    public class MyEventArgs : EventArgs
    {
        public string Message { get; }

        public MyEventArgs(string message)
        {

            Message = message;
        }
    }

}
