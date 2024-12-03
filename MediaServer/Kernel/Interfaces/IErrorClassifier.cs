using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Kernel.Interfaces
{
    public interface IErrorClassifier
    {
        ErrorSeverity ClassifySeverity(Exception ex);
        ErrorCategory ClassifyCategory(Exception ex);
        RecoveryStrategy DetermineRecoveryStrategy(Exception ex);
    }
}
