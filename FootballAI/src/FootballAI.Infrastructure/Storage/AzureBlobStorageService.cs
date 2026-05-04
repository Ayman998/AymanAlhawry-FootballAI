using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using FootballAI.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FootballAI.src.FootballAI.Infrastructure.Storage;

public class AzureBlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _container;
    private readonly ILogger<AzureBlobStorageService> _logger;

    public AzureBlobStorageService(
        string connectionString,
        string containerName,
        ILogger<AzureBlobStorageService> logger)
    {
        _logger = logger;
        var serviceClient = new BlobServiceClient(connectionString);
        _container = serviceClient.GetBlobContainerClient(containerName);
        _container.CreateIfNotExists(PublicAccessType.None);
    }

    public async Task<string> UploadAsync(
     IFormFile file, string blobName, CancellationToken ct = default)
    {
        var blobClient = _container.GetBlobClient(blobName);
        using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(stream,
            new BlobHttpHeaders { ContentType = file.ContentType }, cancellationToken: ct);
        _logger.LogInformation("Uploaded {Blob} ({Size} bytes)", blobName, file.Length);
        return blobClient.Uri.ToString();
    }

    public async Task<string> UploadAsync(
      Stream stream, string blobName, string contentType, CancellationToken ct = default)
    {
        var blobClient = _container.GetBlobClient(blobName);
        await blobClient.UploadAsync(stream,
            new BlobHttpHeaders { ContentType = contentType }, cancellationToken: ct);
        return blobClient.Uri.ToString();
    }

    public async Task<Stream> DownloadAsync(string blobName, CancellationToken ct = default)
    {
        var blobClient = _container.GetBlobClient(blobName);
        var response = await blobClient.DownloadStreamingAsync(cancellationToken: ct);
        return response.Value.Content;
    }

    public async Task DeleteAsync(string blobName, CancellationToken ct = default)
    {
        var blobClient = _container.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
    }


    public Task<string> GetSignedUrlAsync(string blobName, TimeSpan expiry)
    {
        var blobClient = _container.GetBlobClient(blobName);
        if (!blobClient.CanGenerateSasUri)
            throw new InvalidOperationException("Blob client cannot generate SAS URIs");

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _container.Name,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.Add(expiry)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);
        return Task.FromResult(blobClient.GenerateSasUri(sasBuilder).ToString());
    }
}
