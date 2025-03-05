using Azure.Storage.Blobs;
using System.Text;
using Application.Common.Interfaces;
using Azure;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace Infrastructure.Repositories;

public sealed class AzureBlobRepository : IAzureBlobRepository
{
    private readonly BlobContainerClient _containerClient;

    public AzureBlobRepository(BlobContainerClient containerClient)
    {
        _containerClient = containerClient;
    }

    public async Task UploadBlobAsync(string blobName, string content)
    {
        await _containerClient.CreateIfNotExistsAsync();

        var blobClient = _containerClient.GetBlobClient(blobName);

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        await blobClient.UploadAsync(stream, overwrite: true);
    }

    public async Task<string> DownloadBlobAsync(string blobName)
    {
        await _containerClient.CreateIfNotExistsAsync();

        var blobClient = _containerClient.GetBlobClient(blobName);

        var response = await blobClient.DownloadAsync();
        
        using var reader = new StreamReader(response.Value.Content);
        
        return await reader.ReadToEndAsync();
    }

    public async Task DeleteBlobAsync(string blobName)
    {
        await _containerClient.CreateIfNotExistsAsync();

        var blobClient = _containerClient.GetBlobClient(blobName);

        await blobClient.DeleteIfExistsAsync();
    }

    public async Task<Stream> GetAsStream(string blobName)
    {
        await _containerClient.CreateIfNotExistsAsync();

        var blobClient = _containerClient.GetBlobClient(blobName);

        Response<BlobDownloadStreamingResult> response = await blobClient.DownloadStreamingAsync();

        return response.Value.Content;
    }
}