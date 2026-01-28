using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StockMind.Application.Interfaces;

namespace StockMind.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IHostEnvironment _environment;
    private readonly ILogger<LocalFileStorageService> _logger;
    private const string UploadFolder = "uploads";
    private const string ProductImagesFolder = "products";

    public LocalFileStorageService(
        IHostEnvironment environment,
        ILogger<LocalFileStorageService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException($"File type {extension} is not allowed. Allowed types: {string.Join(", ", allowedExtensions)}");
            }

            // Validate file size (max 5MB)
            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            if (fileStream.Length > maxFileSize)
            {
                throw new InvalidOperationException($"File size exceeds the maximum allowed size of {maxFileSize / (1024 * 1024)}MB");
            }

            // Generate unique file name
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";

            // Create upload directory if it doesn't exist
            var basePath = _environment.ContentRootPath;
            var uploadPath = Path.Combine(basePath, "wwwroot", UploadFolder, ProductImagesFolder);
            Directory.CreateDirectory(uploadPath);

            // Save file
            var filePath = Path.Combine(uploadPath, uniqueFileName);
            using (var fileStreamOutput = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileStreamOutput, cancellationToken);
            }

            // Return relative URL
            var fileUrl = $"/{UploadFolder}/{ProductImagesFolder}/{uniqueFileName}";
            
            _logger.LogInformation("File uploaded successfully: {FileUrl}", fileUrl);
            
            return fileUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", fileName);
            throw;
        }
    }

    public Task<bool> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
            {
                return Task.FromResult(false);
            }

            // Remove leading slash
            var relativePath = fileUrl.TrimStart('/');
            var basePath = _environment.ContentRootPath;
            var filePath = Path.Combine(basePath, "wwwroot", relativePath);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("File deleted successfully: {FileUrl}", fileUrl);
                return Task.FromResult(true);
            }

            _logger.LogWarning("File not found for deletion: {FileUrl}", fileUrl);
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FileUrl}", fileUrl);
            return Task.FromResult(false);
        }
    }

    public Task<bool> FileExistsAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
            {
                return Task.FromResult(false);
            }

            var relativePath = fileUrl.TrimStart('/');
            var basePath = _environment.ContentRootPath;
            var filePath = Path.Combine(basePath, "wwwroot", relativePath);

            return Task.FromResult(File.Exists(filePath));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence: {FileUrl}", fileUrl);
            return Task.FromResult(false);
        }
    }
}
