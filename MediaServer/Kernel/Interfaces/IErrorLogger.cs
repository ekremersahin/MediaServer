using MediaServer.Kernel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Kernel.Interfaces
{
    public interface IErrorLogger
    {
        Task LogErrorAsync(ErrorDetails errorDetails);
        Task<IEnumerable<ErrorDetails>> GetRecentErrorsAsync(TimeSpan period);
    }
}
