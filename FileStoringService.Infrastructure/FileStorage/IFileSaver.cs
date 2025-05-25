using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStoringService.Infrastructure.FileStorage
{
    public interface IFileSaver
    {
        Task<string> SaveFileAsync(Stream stream, string originalFileName);
        Task<Stream?> GetFileAsync(string storagePath);
        Task DeleteFileAsync(string storagePath);
    }
}