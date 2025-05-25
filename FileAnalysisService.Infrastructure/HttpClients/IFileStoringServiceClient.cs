using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileAnalysisService.Infrastructure.HttpClients
{
    public interface IFileStoringServiceClient
    {
        Task<Stream?> GetFileContentAsync(Guid fileId);
    }
}