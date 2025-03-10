namespace Application.Common.Interfaces;

public interface IAzureBlobRepository
{
    Task UploadBlobAsync(string blobName, string content);
    Task<string> DownloadBlobAsync(string blobName);
    Task DeleteBlobAsync(string blobName);
    Task<Stream> GetAsStream(string blobName);
}