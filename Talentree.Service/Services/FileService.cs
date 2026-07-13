using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Talentree.Core.Exceptions;
using Talentree.Service.Contracts;

namespace Talentree.Service.Services
{
    public class FileService : IFileService
    {
        private readonly Cloudinary _cloudinary;

        public FileService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new BadRequestException("No file provided");

            using var stream = file.OpenReadStream();

            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = $"talentree/{folder}"
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
                throw new Exception($"Cloudinary upload failed: {result.Error.Message}");

            return result.SecureUrl.ToString();
        }

        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
                return false;

            // Only process Cloudinary URLs
            if (!fileUrl.Contains("cloudinary.com"))
                return false;

            try
            {
                var publicId = ExtractPublicId(fileUrl);
                if (string.IsNullOrEmpty(publicId))
                    return false;

                var result = await _cloudinary.DestroyAsync(new DeletionParams(publicId)
                {
                    ResourceType = ResourceType.Raw
                });

                return result.Result == "ok";
            }
            catch
            {
                return false;
            }
        }

        public string GetFileSizeMB(long bytes)
        {
            return $"{bytes / 1024.0 / 1024.0:F2} MB";
        }

        /// <summary>
        /// Extracts the Cloudinary public ID from a full URL for raw files.
        /// </summary>
        private static string? ExtractPublicId(string fileUrl)
        {
            try
            {
                var uri = new Uri(fileUrl);
                var path = uri.AbsolutePath;

                var uploadIndex = path.IndexOf("/upload/");
                if (uploadIndex < 0) return null;

                var afterUpload = path[(uploadIndex + 8)..];

                // Skip the version segment (v123/)
                var slashIndex = afterUpload.IndexOf('/');
                if (slashIndex < 0) return null;

                // For raw files, keep the extension in the public ID
                return afterUpload[(slashIndex + 1)..];
            }
            catch
            {
                return null;
            }
        }
    }
}