using Document_Management_System.Interfaces;

namespace Document_Management_System.Services
{
    public class LocalFileStorageService: IFileStorageService
    {
        // Storing outside wwwroot so files aren't directly served as static
        // content — every file access should go through a controller that
        // checks permissions first
        private readonly string _storagePath;

        public LocalFileStorageService(IWebHostEnvironment env)
        {
            _storagePath = Path.Combine(env.ContentRootPath, "DocumentStorage");
            if (!Directory.Exists(_storagePath))
                Directory.CreateDirectory(_storagePath);
        }

        public async Task<string> SaveFileAsync(Stream fileStream, string fileName)
        {
            // Prefix with a GUID so two uploads with the same original filename
            // never collide or overwrite each other on disk
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var fullPath = Path.Combine(_storagePath, uniqueFileName);

            using (var output = new FileStream(fullPath, FileMode.Create))
            {
                await fileStream.CopyToAsync(output);
            }

            return uniqueFileName; // stored in DocumentVersion.FilePath; kept relative, not absolute
        }

        public Task<Stream> GetFileAsync(string filePath)
        {
            var fullPath = Path.Combine(_storagePath, filePath);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException("Document file not found.", filePath);

            Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            return Task.FromResult(stream);
        }

        public Task DeleteFileAsync(string filePath)
        {
            var fullPath = Path.Combine(_storagePath, filePath);
            if (File.Exists(fullPath))
                File.Delete(fullPath);

            return Task.CompletedTask;
        }
    }
}
