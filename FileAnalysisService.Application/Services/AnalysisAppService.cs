using FileAnalysisService.Domain.Entities;
using FileAnalysisService.Infrastructure.Persistence;
using FileAnalysisService.Infrastructure.HttpClients;
using FileAnalysisService.Application.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FileAnalysisService.Application.Services
{
    public class AnalysisAppService
    {
        private readonly FileAnalysisDbContext _dbContext;
        private readonly IFileStoringServiceClient _fileStoringClient;
        private readonly ILogger<AnalysisAppService> _logger;

        public AnalysisAppService(
            FileAnalysisDbContext dbContext,
            IFileStoringServiceClient fileStoringClient,
            ILogger<AnalysisAppService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _fileStoringClient = fileStoringClient ?? throw new ArgumentNullException(nameof(fileStoringClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AnalysisResultDto?> GetOrPerformAnalysisAsync(Guid fileId, bool isNewContent)
        {
            _logger.LogInformation("Analysis requested for FileId: {FileId}. IsNewContent (from FileStoringSvc): {IsNewContent}", fileId, isNewContent);

            var existingAnalysis = await _dbContext.AnalysisResults
                                                 .AsNoTracking()
                                                 .FirstOrDefaultAsync(ar => ar.FileId == fileId);

            if (existingAnalysis != null)
            {
                _logger.LogInformation("Found existing analysis result for FileId: {FileId}. Returning cached data.", fileId);
                return MapToDto(existingAnalysis);
            }

            _logger.LogInformation("No existing analysis for FileId: {FileId}. Performing new analysis.", fileId);

            Stream? fileStream = await _fileStoringClient.GetFileContentAsync(fileId);
            if (fileStream == null)
            {
                _logger.LogError("Could not retrieve file content for FileId: {FileId} from FileStoringService. Analysis cannot proceed.", fileId);
                return null;
            }

            string fileContent;
            using (var reader = new StreamReader(fileStream))
            {
                fileContent = await reader.ReadToEndAsync();
            }

            fileStream.Dispose();

            var (paragraphs, words, characters) = TextAnalyzer.Analyze(fileContent);
            _logger.LogDebug("Text analysis for FileId: {FileId} - Paragraphs: {P}, Words: {W}, Chars: {C}", fileId, paragraphs, words, characters);

            var newAnalysis = new AnalysisResult
            {
                Id = Guid.NewGuid(),
                FileId = fileId,
                ParagraphCount = paragraphs,
                WordCount = words,
                CharacterCount = characters,
                IsDuplicateContent = !isNewContent,
                AnalysisTimestamp = DateTime.UtcNow,
            };

            _dbContext.AnalysisResults.Add(newAnalysis);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("New analysis result saved for FileId: {FileId}. IsDuplicateContent: {IsDuplicate}", fileId, newAnalysis.IsDuplicateContent);

            return MapToDto(newAnalysis);
        }

        public async Task<AnalysisResultDto?> GetAnalysisResultAsync(Guid fileId)
        {
            _logger.LogDebug("Fetching analysis result for FileId: {FileId}", fileId);
            var analysis = await _dbContext.AnalysisResults
                                           .AsNoTracking()
                                           .FirstOrDefaultAsync(ar => ar.FileId == fileId);
            if (analysis == null)
            {
                _logger.LogWarning("Analysis result not found for FileId: {FileId}", fileId);
                return null;
            }
            return MapToDto(analysis);
        }

        private AnalysisResultDto MapToDto(AnalysisResult analysis)
        {
            return new AnalysisResultDto
            {
                Id = analysis.Id,
                FileId = analysis.FileId,
                ParagraphCount = analysis.ParagraphCount,
                WordCount = analysis.WordCount,
                CharacterCount = analysis.CharacterCount,
                IsDuplicateContent = analysis.IsDuplicateContent,
                AnalysisTimestamp = analysis.AnalysisTimestamp
            };
        }
    }
}