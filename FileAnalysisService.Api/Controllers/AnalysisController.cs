using Microsoft.AspNetCore.Mvc;
using FileAnalysisService.Application.Services;
using FileAnalysisService.Application.Dtos;      

namespace FileAnalysisService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalysisController : ControllerBase
    {
        private readonly AnalysisAppService _analysisAppService;
        private readonly ILogger<AnalysisController> _logger;

        public AnalysisController(AnalysisAppService analysisAppService, ILogger<AnalysisController> logger)
        {
            _analysisAppService = analysisAppService ?? throw new ArgumentNullException(nameof(analysisAppService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("{fileId:guid}")]
        [ProducesResponseType(typeof(AnalysisResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RequestAnalysis(Guid fileId, [FromBody] AnalysisRequestDto requestDto)
        {
            _logger.LogInformation("RequestAnalysis called for FileId: {FileId}. IsNewContent: {IsNewContent}", fileId, requestDto.IsNewContent);

            if (fileId == Guid.Empty)
            {
                _logger.LogWarning("RequestAnalysis: Invalid FileId (empty GUID) provided.");
                ModelState.AddModelError("fileId", "FileId cannot be empty.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            
            if (requestDto == null)
            {
                _logger.LogWarning("RequestAnalysis: Request body (AnalysisRequestDto) is null for FileId: {FileId}", fileId);
                ModelState.AddModelError(string.Empty, "Request body is required.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            try
            {
                var result = await _analysisAppService.GetOrPerformAnalysisAsync(fileId, requestDto.IsNewContent);

                if (result == null)
                {
                    _logger.LogWarning("RequestAnalysis: Analysis could not be performed or underlying file not found for FileId: {FileId}", fileId);
                    return NotFound(new ProblemDetails { Title = "Analysis Failed or File Not Found", Detail = $"Could not perform analysis. The file with ID '{fileId}' might not exist or is inaccessible.", Status = StatusCodes.Status404NotFound });
                }

                _logger.LogInformation("RequestAnalysis for FileId: {FileId} completed successfully.", fileId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RequestAnalysis: Unexpected error during analysis for FileId: {FileId}", fileId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "Unexpected Analysis Error", Detail = "An unexpected server error occurred during file analysis.", Status = StatusCodes.Status500InternalServerError });
            }
        }


        [HttpGet("results/{fileId:guid}")]
        [ProducesResponseType(typeof(AnalysisResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAnalysisResults(Guid fileId)
        {
            _logger.LogInformation("GetAnalysisResults called for FileId: {FileId}", fileId);

            if (fileId == Guid.Empty)
            {
                _logger.LogWarning("GetAnalysisResults: Invalid FileId (empty GUID) provided.");
                return BadRequest(new ProblemDetails { Title = "Invalid File ID", Detail = "File ID cannot be empty.", Status = StatusCodes.Status400BadRequest });
            }

            try
            {
                var result = await _analysisAppService.GetAnalysisResultAsync(fileId);

                if (result == null)
                {
                    _logger.LogWarning("GetAnalysisResults: Analysis results not found for FileId: {FileId}. Analysis might not have been performed yet.", fileId);
                    return NotFound(new ProblemDetails { Title = "Analysis Results Not Found", Detail = $"Analysis results for file ID '{fileId}' not found. Please ensure analysis has been requested and completed.", Status = StatusCodes.Status404NotFound });
                }

                _logger.LogInformation("GetAnalysisResults for FileId: {FileId} completed successfully.", fileId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAnalysisResults: Unexpected error retrieving analysis results for FileId: {FileId}", fileId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "Unexpected Error", Detail = "An unexpected server error occurred while retrieving analysis results.", Status = StatusCodes.Status500InternalServerError });
            }
        }
    }
}