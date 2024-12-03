using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Kernel.Interfaces
{
    public interface IFileService
    {
        Task<string[]> GetFilesAsync(string directory, string searchPattern);
        Task DeleteFileAsync(string filePath);
        long GetFileSizeMB(string filePath);
        DateTime GetLastWriteTimeUtc(string filePath);
    }
}
