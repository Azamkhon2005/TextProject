using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace FileStoringService.Infrastructure.FileStorage
{
    public class LocalFileSaver : IFileSaver
    {
        private readonly string _basePath;
        private readonly ILogger<LocalFileSaver> _logger;

        public LocalFileSaver(IOptions<LocalFileSaverOptions> options, ILogger<LocalFileSaver> logger)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (options.Value == null) throw new ArgumentNullException(nameof(options.Value));

            _basePath = options.Value.BasePath;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (string.IsNullOrWhiteSpace(_basePath))
            {
                _basePath = "uploads_fss_default";
                _logger.LogWarning("BasePath for LocalFileSaver was not configured or empty. Using default: '{DefaultPath}'.", _basePath);
            }

            try
            {
                if (!Directory.Exists(_basePath))
                {
                    Directory.CreateDirectory(_basePath);
                    _logger.LogInformation("Created storage directory at: '{StoragePath}'.", Path.GetFullPath(_basePath));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create or access storage directory at '{StoragePath}'. Please check permissions and configuration.", Path.GetFullPath(_basePath));
                
                throw new InvalidOperationException($"Failed to initialize LocalFileSaver: Could not create or access base path '{_basePath}'.", ex);
            }
        }

        public async Task<string> SaveFileAsync(Stream stream, string originalFileName)
        {
            var fileExtension = Path.GetExtension(originalFileName);
            var uniqueFileNameInStorage = $"{Guid.NewGuid()}{fileExtension}";
            var dateSubFolder = DateTime.UtcNow.ToString("yyyyMMdd");
            var relativeFolderPath = Path.Combine(dateSubFolder);
            var relativeFilePath = Path.Combine(relativeFolderPath, uniqueFileNameInStorage);

            var absoluteFolderPath = Path.Combine(_basePath, relativeFolderPath);
            var absoluteFilePath = Path.Combine(_basePath, relativeFilePath);


            _logger.LogInformation("Attempting to save file. Original: '{OriginalFileName}', StorageName: '{StorageFileName}', Path: '{FilePath}'",
                                   originalFileName, uniqueFileNameInStorage, absoluteFilePath);
            try
            {
                if (!Directory.Exists(absoluteFolderPath))
                {
                    Directory.CreateDirectory(absoluteFolderPath);
                    _logger.LogInformation("Created sub-directory for storage: '{DirectoryPath}'", absoluteFolderPath);
                }

                if (stream.CanSeek)
                {
                    stream.Position = 0;
                }

                using (var fileStream = new FileStream(absoluteFilePath, FileMode.Create, FileAccess.Write))
                {
                    await stream.CopyToAsync(fileStream);
                }
                _logger.LogInformation("Successfully saved file to: '{FilePath}'.", absoluteFilePath);
                return relativeFilePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file '{OriginalFileName}' to '{FilePath}'.", originalFileName, absoluteFilePath);
                throw;
            }
        }

        public Task<Stream?> GetFileAsync(string storagePath)
        {
            var absoluteFilePath = Path.Combine(_basePath, storagePath);
            _logger.LogDebug("Attempting to get file from: '{FilePath}'.", absoluteFilePath);

            if (!File.Exists(absoluteFilePath))
            {
                _logger.LogWarning("File not found at: '{FilePath}'.", absoluteFilePath);
                return Task.FromResult<Stream?>(null);
            }
            try
            {
                return Task.FromResult<Stream?>(new FileStream(absoluteFilePath, FileMode.Open, FileAccess.Read, FileShare.Read));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening file from: '{FilePath}'.", absoluteFilePath);
                return Task.FromResult<Stream?>(null);
            }
        }

        public Task DeleteFileAsync(string storagePath)
        {
            var absoluteFilePath = Path.Combine(_basePath, storagePath);
            _logger.LogInformation("Attempting to delete file from: '{FilePath}'.", absoluteFilePath);
            try
            {
                if (File.Exists(absoluteFilePath))
                {
                    File.Delete(absoluteFilePath);
                    _logger.LogInformation("Successfully deleted file: '{FilePath}'.", absoluteFilePath);
                }
                else
                {
                    _logger.LogWarning("File not found for deletion at: '{FilePath}'.", absoluteFilePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file from: '{FilePath}'.", absoluteFilePath);
            }
            return Task.CompletedTask;
        }
    }
}
