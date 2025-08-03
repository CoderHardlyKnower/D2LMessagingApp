namespace MessagingApp.Services
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _uploadPath;

        public LocalFileStorageService(IWebHostEnvironment env)
        {
            // wwwroot/uploads
            _uploadPath = Path.Combine(env.WebRootPath, "uploads");
            if (!Directory.Exists(_uploadPath))
                Directory.CreateDirectory(_uploadPath);
        }

        public Task<string> UploadAsync(Stream fileStream, string fileName)
        {
            // give each file a GUID prefix
            var uniqueName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
            var fullPath = Path.Combine(_uploadPath, uniqueName);

            using var fs = new FileStream(fullPath, FileMode.Create);
            fileStream.CopyTo(fs);

            // return a public URL path
            return Task.FromResult($"/uploads/{uniqueName}");
        }
    }
}
