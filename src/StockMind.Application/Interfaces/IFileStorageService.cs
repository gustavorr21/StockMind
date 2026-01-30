namespace StockMind.Application.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Upload a file and return the URL
    /// </summary>
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete a file by URL
    /// </summary>
    Task<bool> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if file exists
    /// </summary>
    Task<bool> FileExistsAsync(string fileUrl, CancellationToken cancellationToken = default);
}
