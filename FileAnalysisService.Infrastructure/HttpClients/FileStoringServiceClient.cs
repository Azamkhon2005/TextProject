using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace FileAnalysisService.Infrastructure.HttpClients
{
    public class FileStoringServiceOptions
    {
        public const string SectionName = "ServiceEndpoints:FileStoringService";
        public string BaseUrl { get; set; } = string.Empty;
    }

    public class FileStoringServiceClient : IFileStoringServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FileStoringServiceClient> _logger;

        public FileStoringServiceClient(HttpClient httpClient, ILogger<FileStoringServiceClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Stream?> GetFileContentAsync(Guid fileId)
        {
            string requestUri = $"api/files/{fileId}/content";
            _logger.LogInformation("Requesting file content from FileStoringService. URI: '{RequestUri}'", _httpClient.BaseAddress + requestUri);

            try
            {
                var response = await _httpClient.GetAsync(requestUri);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully retrieved file stream for FileId: {FileId}", fileId);
                    return await response.Content.ReadAsStreamAsync();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to get file content for FileId: {FileId}. Status: {StatusCode}. Response: {ErrorContent}",
                                       fileId, response.StatusCode, errorContent);
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while getting file content for FileId: {FileId} from {BaseUrl}{RequestUri}",
                                 fileId, _httpClient.BaseAddress, requestUri);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting file content for FileId: {FileId} from {BaseUrl}{RequestUri}",
                                 fileId, _httpClient.BaseAddress, requestUri);
                return null;
            }
        }
    }
}
