namespace SterlingAuctions.Core.Interfaces;

/// <summary>
/// Service interface for file storage operations
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Upload file to storage
    /// </summary>
    Task<(bool Success, string? FileUrl, IEnumerable<string> Errors)> UploadFileAsync(
        Stream fileStream, string fileName, string contentType, string? folder = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upload image with automatic resizing
    /// </summary>
    Task<(bool Success, string? OriginalUrl, string? ThumbnailUrl, string? MediumUrl, IEnumerable<string> Errors)> 
        UploadImageAsync(Stream imageStream, string fileName, string contentType, string? folder = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete file from storage
    /// </summary>
    Task<bool> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete multiple files from storage
    /// </summary>
    Task<bool> DeleteFilesAsync(IEnumerable<string> fileUrls, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get file download URL
    /// </summary>
    Task<string?> GetDownloadUrlAsync(string fileKey, TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if file exists
    /// </summary>
    Task<bool> FileExistsAsync(string fileUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get file metadata
    /// </summary>
    Task<FileMetadata?> GetFileMetadataAsync(string fileUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Copy file
    /// </summary>
    Task<(bool Success, string? NewUrl, IEnumerable<string> Errors)> CopyFileAsync(
        string sourceUrl, string destinationKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Move file
    /// </summary>
    Task<(bool Success, string? NewUrl, IEnumerable<string> Errors)> MoveFileAsync(
        string sourceUrl, string destinationKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get storage usage for a folder
    /// </summary>
    Task<StorageUsage> GetStorageUsageAsync(string? folder = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// File metadata information
/// </summary>
public class FileMetadata
{
    public string FileName { get; set; } = string.Empty;
    public long Size { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
    public string ETag { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Storage usage information
/// </summary>
public class StorageUsage
{
    public long TotalFiles { get; set; }
    public long TotalSize { get; set; }
    public DateTime LastCalculated { get; set; }
    public Dictionary<string, long> FilesByType { get; set; } = new();
}
