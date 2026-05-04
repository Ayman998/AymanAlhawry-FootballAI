using Microsoft.AspNetCore.Http;

namespace FootballAI.Application.Interfaces;

public interface IBlobStorageService
{
    Task<string> UploadAsync(IFormFile file, string blobName, CancellationToken ct = default);
    Task<string> UploadAsync(Stream stream, string blobName, string contentType, CancellationToken ct = default);
    Task<Stream> DownloadAsync(string blobName, CancellationToken ct = default);
    Task DeleteAsync(string blobName, CancellationToken ct = default);
    Task<string> GetSignedUrlAsync(string blobName, TimeSpan expiry);
}
