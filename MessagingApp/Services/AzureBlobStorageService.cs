using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.StaticFiles;

namespace MessagingApp.Services
{
    public class AzureBlobStorageService : IFileStorageService
    {
        private readonly BlobServiceClient _blobClient;

        public AzureBlobStorageService(BlobServiceClient blobClient)
        {
            _blobClient = blobClient;
        }

        public async Task<string> UploadAsync(Stream fileStream, string fileName)
        {
            var container = _blobClient.GetBlobContainerClient("message-files");

            // Make container if missing/non exist, allow blob-level public access (read-only)
            await container.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var ext = Path.GetExtension(fileName);
            var blob = container.GetBlobClient($"{Guid.NewGuid()}{ext}");

            var contentType = GetContentType(fileName);
            var headers = new BlobHttpHeaders { ContentType = contentType };

            await blob.UploadAsync(fileStream, new BlobUploadOptions { HttpHeaders = headers });

            return blob.Uri.ToString(); 
        }

        private static string GetContentType(string fileName)
        {
            var provider = new FileExtensionContentTypeProvider();
            return provider.TryGetContentType(fileName, out var ct) ? ct : "application/octet-stream";
        }
    }
}
