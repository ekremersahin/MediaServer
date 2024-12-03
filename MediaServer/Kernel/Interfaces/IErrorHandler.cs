using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Kernel.Interfaces
{
    public interface IErrorHandler
    {
        Task<RecoveryStrategy> HandleErrorAsync(
            Exception ex,
            RecoveryStrategy defaultStrategy = RecoveryStrategy.LogAndContinue,
            [CallerMemberName] string callerMethod = "",
            [CallerFilePath] string callerFile = "");
    }


}
