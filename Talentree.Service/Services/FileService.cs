using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Talentree.Core.Exceptions;
using Talentree.Service.Contracts;

namespace Talentree.Service.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new BadRequestException("No file provided");

            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", folder);
            Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{folder}/{fileName}";
        }

        public Task<bool> DeleteFileAsync(string fileUrl)
        {
            try
            {
                var filePath = Path.Combine(_environment.WebRootPath, fileUrl.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public string GetFileSizeMB(long bytes)
        {
            return $"{bytes / 1024.0 / 1024.0:F2} MB";
        }
    }
}