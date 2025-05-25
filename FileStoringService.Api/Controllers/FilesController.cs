using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// FileStoringService.Api/Controllers/FilesController.cs
using Microsoft.AspNetCore.Mvc;
using FileStoringService.Application.Services;      // FileAppService
using FileStoringService.Application.Dtos;          // FileUploadResultDto
using FileStoringService.Api.Dtos;                  // FileUploadControllerResponseDto (если используется)
// using System.ComponentModel.DataAnnotations; // Для атрибутов валидации, если нужны на DTO запроса

namespace FileStoringService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // -> /api/files
    public class FilesController : ControllerBase
    {
        private readonly FileAppService _fileAppService;
        private readonly ILogger<FilesController> _logger;

        public FilesController(FileAppService fileAppService, ILogger<FilesController> logger)
        {
            _fileAppService = fileAppService ?? throw new ArgumentNullException(nameof(fileAppService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        [ProducesResponseType(typeof(FileUploadControllerResponseDto), StatusCodes.Status200OK)] // Используем DTO из Api слоя
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [RequestSizeLimit(10 * 1024 * 1024)] // Ограничение размера запроса на уровне контроллера/действия (10MB)
                                             // Также можно настроить на уровне Kestrel
        public async Task<IActionResult> UploadFile(IFormFile file) // IFormFile для загрузки файла
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("UploadFile: No file provided or file is empty.");
                ModelState.AddModelError("file", "No file provided or file is empty.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (extension != ".txt")
            {
                _logger.LogWarning("UploadFile: Invalid file type '{Extension}'. Only .txt is allowed. FileName: {FileName}", extension, file.FileName);
                ModelState.AddModelError("file", "Invalid file type. Only .txt files are allowed.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            // Дополнительная проверка на размер файла, хотя RequestSizeLimit уже должен был сработать
            const long maxFileSize = 10 * 1024 * 1024; // 10 MB (согласовать с RequestSizeLimit)
            if (file.Length > maxFileSize)
            {
                _logger.LogWarning("UploadFile: File '{FileName}' (Size: {FileSize}) exceeds maximum allowed size of {MaxFileSize} bytes.",
                                   file.FileName, file.Length, maxFileSize);
                ModelState.AddModelError("file", $"File exceeds maximum size of {maxFileSize / (1024 * 1024)}MB.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            _logger.LogInformation("UploadFile: Attempting to process file '{FileName}', Size: {FileSize}, ContentType: '{ContentType}'",
                                   file.FileName, file.Length, file.ContentType);
            try
            {
                // IFormFile.OpenReadStream() создает поток, который можно читать.
                // Для .NET Core 3.0+ он обычно поддерживает CanSeek.
                using var stream = file.OpenReadStream();

                FileUploadResultDto appServiceResult = await _fileAppService.UploadFileAsync(
                    stream,
                    file.FileName,
                    file.ContentType,
                    file.Length);

                // Маппинг из Application DTO в Api DTO (если они разные)
                var responseDto = new FileUploadControllerResponseDto
                {
                    FileId = appServiceResult.FileId,
                    IsNew = appServiceResult.IsNew, // Всегда true
                    OriginalFileName = appServiceResult.OriginalFileName
                };

                _logger.LogInformation("UploadFile: File '{FileName}' processed successfully. FileId: {FileId}",
                                       file.FileName, responseDto.FileId);
                return Ok(responseDto);
            }
            catch (ArgumentException ex) // От FileAppService
            {
                _logger.LogWarning(ex, "UploadFile: Argument error for file '{FileName}' during app service call.", file.FileName);
                return BadRequest(new ProblemDetails { Title = "Invalid request argument", Detail = ex.Message, Status = StatusCodes.Status400BadRequest });
            }
            catch (InvalidOperationException ex) // От FileAppService или IFileSaver
            {
                _logger.LogError(ex, "UploadFile: Operation error for file '{FileName}'.", file.FileName);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "Operation error", Detail = "An internal error occurred while processing the file.", Status = StatusCodes.Status500InternalServerError });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UploadFile: Unexpected error processing file '{FileName}'", file.FileName);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "Unexpected error", Detail = "An unexpected server error occurred.", Status = StatusCodes.Status500InternalServerError });
            }
        }

        [HttpGet("{id:guid}/content")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFileContent(Guid id)
        {
            _logger.LogInformation("GetFileContent: Request for FileId: {FileId}", id);

            if (id == Guid.Empty)
            {
                _logger.LogWarning("GetFileContent: Invalid FileId (empty GUID) requested.");
                return BadRequest(new ProblemDetails { Title = "Invalid File ID", Detail = "File ID cannot be empty.", Status = StatusCodes.Status400BadRequest });
            }

            try
            {
                var (fileStream, metadata) = await _fileAppService.GetFileContentAndMetadataAsync(id);

                if (fileStream == null || metadata == null)
                {
                    _logger.LogWarning("GetFileContent: File or metadata not found for FileId: {FileId}", id);
                    return NotFound(new ProblemDetails { Title = "File Not Found", Detail = $"File with ID '{id}' not found.", Status = StatusCodes.Status404NotFound });
                }

                _logger.LogInformation("GetFileContent: Returning file '{FileName}' for FileId: {FileId}", metadata.OriginalFileName, id);
                // FileStreamResult сам позаботится о закрытии потока
                return File(fileStream, metadata.ContentType, metadata.OriginalFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetFileContent: Error retrieving file for FileId: {FileId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "Error retrieving file", Detail = "An unexpected server error occurred while retrieving the file.", Status = StatusCodes.Status500InternalServerError });
            }
        }
    }
}
