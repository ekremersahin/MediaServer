using MediaServer.Kernel.Interfaces;
using MediaServer.Kernel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Kernel.Services
{
    public class DefaultErrorNotificationService : IErrorNotificationService
    {
        public Task NotifyAsync(ErrorDetails errorDetails)
        {
            return Task.CompletedTask;
        }
    }
}
