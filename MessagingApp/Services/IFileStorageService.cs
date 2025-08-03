namespace MessagingApp.Services
{
    public interface IFileStorageService
    {
        /// <summary>
        /// Uploads the given stream with the provided filename, returns the public URL
        /// </summary>
        Task<string> UploadAsync(Stream fileStream, string fileName);
    }
}
