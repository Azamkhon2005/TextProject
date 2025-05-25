using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStoringService.Infrastructure.FileStorage
{
    public class LocalFileSaverOptions
    {
        public const string SectionName = "FileStorage";
        public string BasePath { get; set; } = "uploads_fss";
    }
}
