using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Infrastructure.Exceptions;
using Image = SixLabors.ImageSharp.Image;

namespace WebsupplyConnect.Infrastructure.ExternalServices.Storage;

public class BlobStorageService(ILogger<BlobStorageService> logger) : IBlobStorageService
{
    private readonly ILogger<BlobStorageService> _logger = logger;

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string connectionString, string containerName)
    {
        try
        {
            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Garante que o container exista
            var exists = await containerClient.ExistsAsync();
            if (!exists.Value)
            {
                await containerClient.CreateAsync();
            }

            var blobClient = containerClient.GetBlobClient(fileName);

            // Faz upload do arquivo
            await blobClient.UploadAsync(fileStream, overwrite: true);

            // Define o content-type do arquivo
            await blobClient.SetHttpHeadersAsync(new Azure.Storage.Blobs.Models.BlobHttpHeaders
            {
                ContentType = contentType
            });

            // Gera URL com SAS Token válida por 1 hora
            if (blobClient.CanGenerateSasUri)
            {
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = containerName,
                    BlobName = fileName,
                    Resource = "b", // 'b' = blob
                    ExpiresOn = DateTimeOffset.UtcNow.AddDays(30)
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                var sasUri = blobClient.GenerateSasUri(sasBuilder);

                return sasUri.ToString(); // URL com token de acesso temporário
            }
            else
            {
                throw new InvalidOperationException("Não é possível gerar a SAS URI. Verifique se está usando credencial com chave de conta.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Erro ao enviar mídia para o blob storage: {erro}", ex);
            throw new InfraException($"Erro ao enviar mídia para blob storage: {ex}");
        }
    }

    public async Task<string> CreateThumbnailAsync(Stream fileStream, string fileName, string connectionString, string containerName)
    {
        try
        {
            using var imagem = await Image.LoadAsync(fileStream);


            imagem.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(150, 150)
            }));

            using var thumbnailStream = new MemoryStream();
            await imagem.SaveAsJpegAsync(thumbnailStream);
            thumbnailStream.Position = 0;

            return await UploadThumbnailToBlobAsync(thumbnailStream, fileName, connectionString, containerName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar thumbnail para o arquivo: {FileName}", fileName);
            throw new InfraException($"Erro ao criar thumbnail para o arquivo '{fileName}': {ex.Message}", ex);
        }
    }

    private static async Task<string> UploadThumbnailToBlobAsync(Stream stream, string originalFileName, string connectionString, string containerName)
    {
        var nomeThumbnail = Path.GetFileNameWithoutExtension(originalFileName) + "-thumb.jpg";

        var blobServiceClient = new BlobServiceClient(connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(nomeThumbnail);

        await containerClient.CreateIfNotExistsAsync();
        await blobClient.UploadAsync(stream, overwrite: true);

        if (blobClient.CanGenerateSasUri)
        {
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = nomeThumbnail,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddDays(30)
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);
            return blobClient.GenerateSasUri(sasBuilder).ToString();
        }

        throw new InvalidOperationException("Não foi possível gerar uma SAS URI.");
    }

    public async Task<Stream> DownloadAsync(string blobName, string connectionString, string containerName)
    {

        var blobServiceClient = new BlobServiceClient(connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        if (!await blobClient.ExistsAsync())
            throw new FileNotFoundException($"O arquivo {blobName} não foi encontrado no Blob Storage.");

        var downloadInfo = await blobClient.DownloadAsync();
        return downloadInfo.Value.Content;
    }

}

