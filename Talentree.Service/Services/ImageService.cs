using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Service.Contracts;

namespace Talentree.Service.Services
{
    public class ImageService : IImageService
    {
        private readonly string _baseUploadPath;
        private readonly string _baseUrl;
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB
        private readonly string[] _allowedTypes = { "image/jpeg", "image/png", "image/jpg" };

        public ImageService(IConfiguration configuration)
        {
            _baseUploadPath = configuration["ImageStorage:LocalPath"] ?? "wwwroot/uploads";
            _baseUrl = configuration["ImageStorage:BaseUrl"] ?? "/uploads";
        }

        public bool IsValidImage(IFormFile file)
        {
            if (file.Length > _maxFileSize) return false;
            if (!_allowedTypes.Contains(file.ContentType.ToLower())) return false;
            return true;
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folder)
        {
            var uploadPath = Path.Combine(_baseUploadPath, folder);
            Directory.CreateDirectory(uploadPath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"{_baseUrl}/{folder}/{fileName}";
        }

        public Task DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return Task.CompletedTask;

            var relativePath = imageUrl.Replace(_baseUrl, _baseUploadPath);
            if (File.Exists(relativePath))
                File.Delete(relativePath);

            return Task.CompletedTask;
        }
    }
}
