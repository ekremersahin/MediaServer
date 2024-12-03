using MediaServer.Kernel.Interfaces;
using MediaServer.Kernel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Kernel.Services
{
    public class DefaultErrorLogger : IErrorLogger
    {
        public Task<IEnumerable<ErrorDetails>> GetRecentErrorsAsync(TimeSpan period)
        {
            throw new NotImplementedException();
        }

        public Task LogErrorAsync(ErrorDetails errorDetails)
        {
            return Task.CompletedTask;
        }
    }
}
