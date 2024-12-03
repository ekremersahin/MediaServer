using MediaServer.Kernel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.Kernel.Services
{
    public class FileService : IFileService
    {
        public Task<string[]> GetFilesAsync(string directory, string searchPattern)
        {
            return Task.FromResult(Directory.GetFiles(directory, searchPattern));
        }

        public Task DeleteFileAsync(string filePath)
        {
            File.Delete(filePath);
            return Task.CompletedTask;
        }

        public long GetFileSizeMB(string filePath)
        {
            return new FileInfo(filePath).Length / (1024 * 1024);
        }

        public DateTime GetLastWriteTimeUtc(string filePath)
        {
            return File.GetLastWriteTimeUtc(filePath);
        }
    }
}
