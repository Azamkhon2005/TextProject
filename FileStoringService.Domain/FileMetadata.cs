using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStoringService.Domain.Entities
{
    public class FileMetadata
    {
        public Guid Id { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentHash { get; set; } = string.Empty;
        public string StoragePath { get; set; } = string.Empty; 
        public long FileSize { get; set; }
        public string ContentType { get; set; } = "text/plain";
        public DateTime UploadTimestamp { get; set; }
    }
}
