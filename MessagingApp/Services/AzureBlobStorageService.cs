using Azure.Storage.Blobs;
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
            await container.CreateIfNotExistsAsync();  // ensure container exists
            var blob = container.GetBlobClient($"{Guid.NewGuid()}{Path.GetExtension(fileName)}");
            await blob.UploadAsync(fileStream);
            return blob.Uri.ToString();
        }
    }
}
