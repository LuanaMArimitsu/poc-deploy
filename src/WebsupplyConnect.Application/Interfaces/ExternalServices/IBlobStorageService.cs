namespace WebsupplyConnect.Application.Interfaces.ExternalServices
{
    public interface IBlobStorageService
    {
        Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string connectionString, string containerName);

        Task<string>CreateThumbnailAsync(Stream fileStream, string fileName, string connectionString, string containerName);

        Task<Stream> DownloadAsync(string blobName, string connectionString, string containerName);
    }
}
