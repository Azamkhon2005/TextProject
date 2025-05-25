using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileAnalysisService.Application.Dtos
{
    public class AnalysisResultDto
    {
        public Guid Id { get; set; }
        public Guid FileId { get; set; }
        public int ParagraphCount { get; set; }
        public int WordCount { get; set; }
        public int CharacterCount { get; set; }
        public bool IsDuplicateContent { get; set; }
        public DateTime AnalysisTimestamp { get; set; }
    }
}