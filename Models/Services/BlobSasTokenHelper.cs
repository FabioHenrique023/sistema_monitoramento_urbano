using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace sistema_monitoramento_urbano.Models.Services;

public class BlobSasTokenHelper
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public BlobSasTokenHelper(BlobServiceClient blobServiceClient, string containerName)
    {
        _blobServiceClient = blobServiceClient;
        _containerName = containerName;
    }

    public string? GetBlobUrlWithSas(string? blobPath)
    {
        if (string.IsNullOrWhiteSpace(blobPath))
            return null;

        try
        {
            var container = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blob = container.GetBlobClient(blobPath);

            if (!blob.CanGenerateSasUri)
                return blob.Uri.ToString();

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerName,
                BlobName = blobPath,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasUri = blob.GenerateSasUri(sasBuilder);
            return sasUri.ToString();
        }
        catch
        {
            // Se falhar, retorna a URL sem token
            try
            {
                var container = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blob = container.GetBlobClient(blobPath);
                return blob.Uri.ToString();
            }
            catch
            {
                return null;
            }
        }
    }

    public string? GetBlobUrlWithSasFromFullUrl(string? fullUrl)
    {
        if (string.IsNullOrWhiteSpace(fullUrl))
            return null;

        try
        {
            var uri = new Uri(fullUrl);
            var container = _blobServiceClient.GetBlobContainerClient(_containerName);
            
            // Extrair o caminho do blob da URL completa
            var blobPath = uri.AbsolutePath.TrimStart('/');
            if (blobPath.StartsWith(_containerName + "/", StringComparison.OrdinalIgnoreCase))
            {
                blobPath = blobPath.Substring(_containerName.Length + 1);
            }

            return GetBlobUrlWithSas(blobPath);
        }
        catch
        {
            return fullUrl;
        }
    }
}

