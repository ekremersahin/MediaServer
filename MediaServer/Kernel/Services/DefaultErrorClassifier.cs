using MediaServer.Kernel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Kernel.Services
{
    public class DefaultErrorClassifier : IErrorClassifier
    {
        public ErrorSeverity ClassifySeverity(Exception ex) => ex switch
        {
            SecurityException => ErrorSeverity.Critical,
            UnauthorizedAccessException => ErrorSeverity.High,
            TimeoutException => ErrorSeverity.Medium,
            DirectoryNotFoundException => ErrorSeverity.Critical,
            ArgumentOutOfRangeException => ErrorSeverity.Critical,
            ArgumentNullException => ErrorSeverity.Critical,
            _ => ErrorSeverity.Low
        };

        public ErrorCategory ClassifyCategory(Exception ex) => ex switch
        {
            SecurityException => ErrorCategory.Security,
            WebException => ErrorCategory.Network,
            ArgumentOutOfRangeException => ErrorCategory.DataTypeOrValue,
            DirectoryNotFoundException => ErrorCategory.IO,
            ArgumentNullException => ErrorCategory.DataTypeOrValue,
            UnauthorizedAccessException => ErrorCategory.Security,
            TimeoutException => ErrorCategory.Network,
            _ => ErrorCategory.Unknown
        };

        public RecoveryStrategy DetermineRecoveryStrategy(Exception ex) => ex switch
        {
            SecurityException => RecoveryStrategy.Terminate,
            TimeoutException => RecoveryStrategy.Retry,
            _ => RecoveryStrategy.LogAndContinue
        };
    }
}
