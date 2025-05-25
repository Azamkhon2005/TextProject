
namespace FileStoringService.Api.Dtos
{
    public class FileUploadControllerResponseDto
    {
        public Guid FileId { get; set; }
        public bool IsNew { get; set; }
        public string? OriginalFileName { get; set; }
    }
}