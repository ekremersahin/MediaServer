using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Kernel.Interfaces
{
    // Hata yönetim stratejileri
    public enum RecoveryStrategy
    {
        LogAndContinue,
        Retry,
        Restart,
        Rollback,
        Terminate,
        Escalate
    }

    // Hata seviyeleri
    public enum ErrorSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    // Hata kategorileri
    public enum ErrorCategory
    {
        Configuration,
        Network,
        Security,
        Database,
        Authentication,
        Authorization,
        External,
        DataTypeOrValue,
        IO,
        Unknown
    }
}
