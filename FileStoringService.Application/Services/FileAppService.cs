using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FileStoringService.Domain.Entities;
using FileStoringService.Infrastructure.FileStorage;
using FileStoringService.Infrastructure.Persistence;
using FileStoringService.Application.Dtos;
using Microsoft.Extensions.Logging;
using Shared.Core.Hashing;
using Microsoft.EntityFrameworkCore;

namespace FileStoringService.Application.Services
{
    public class FileAppService
    {
        private readonly FileStoringDbContext _context;
        private readonly IFileSaver _fileSaver;
        private readonly ILogger<FileAppService> _logger;

        public FileAppService(
            FileStoringDbContext context,
            IFileSaver fileSaver,
            ILogger<FileAppService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _fileSaver = fileSaver ?? throw new ArgumentNullException(nameof(fileSaver));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<FileUploadResultDto> UploadFileAsync(Stream fileStream, string originalFileName, string contentType, long fileSize)
        {
            _logger.LogInformation(
                "UploadFileAsync called. FileName: '{OriginalFileName}', Size: {FileSize}, ContentType: '{ContentType}'. Every file is treated as new.",
                originalFileName, fileSize, contentType);

            if (fileStream == null || fileSize == 0)
            {
                _logger.LogWarning("UploadFileAsync: File stream is null or file size is zero for '{OriginalFileName}'.", originalFileName);
                throw new ArgumentException("File stream cannot be null and file size must be greater than zero.", nameof(fileStream));
            }

            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }
            else
            {
                _logger.LogWarning("UploadFileAsync: Stream for '{OriginalFileName}' does not support seeking. Hashing and saving might fail if stream was already read.", originalFileName);
                
            }

            string contentHash = await Sha256Hasher.ComputeHashAsync(fileStream);
            _logger.LogDebug("Computed content hash for '{OriginalFileName}': {ContentHash}", originalFileName, contentHash);

            string storagePath = await _fileSaver.SaveFileAsync(fileStream, originalFileName);
            _logger.LogInformation("File '{OriginalFileName}' saved to storage. RelativePath: '{StoragePath}'", originalFileName, storagePath);

            var newFileMetadata = new FileMetadata
            {
                Id = Guid.NewGuid(),
                OriginalFileName = originalFileName,
                ContentHash = contentHash,
                StoragePath = storagePath,
                FileSize = fileSize,
                ContentType = contentType,
                UploadTimestamp = DateTime.UtcNow
            };

            _context.FileMetadatas.Add(newFileMetadata);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Metadata for new file '{OriginalFileName}' saved to DB. FileId: {FileId}",
                originalFileName, newFileMetadata.Id);

            return new FileUploadResultDto
            {
                FileId = newFileMetadata.Id,
                IsNew = true,
                OriginalFileName = newFileMetadata.OriginalFileName
            };
        }

        public async Task<(Stream? FileStream, FileMetadata? Metadata)> GetFileContentAndMetadataAsync(Guid fileId)
        {
            _logger.LogDebug("GetFileContentAndMetadataAsync called for FileId: {FileId}", fileId);
            var metadata = await _context.FileMetadatas
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == fileId);

            if (metadata == null)
            {
                _logger.LogWarning("File metadata not found for FileId: {FileId}", fileId);
                return (null, null);
            }

            var fileStream = await _fileSaver.GetFileAsync(metadata.StoragePath);
            if (fileStream == null)
            {
                _logger.LogWarning("File content not found in storage for FileId: {FileId} at path: '{StoragePath}'. " +
                                   "Database metadata exists but physical file is missing.", fileId, metadata.StoragePath);
            }
            else
            {
                _logger.LogInformation("File content stream retrieved for FileId: {FileId}, Path: '{StoragePath}'", fileId, metadata.StoragePath);
            }
            return (fileStream, metadata);
        }
    }
}
