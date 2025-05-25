using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStoringService.Application.Dtos
{
    public class FileUploadResultDto
    {
        public Guid FileId { get; set; }

        public bool IsNew { get; set; }
        public string? OriginalFileName { get; set; }
    }
}
