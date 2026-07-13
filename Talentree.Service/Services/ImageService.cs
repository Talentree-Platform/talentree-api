using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Talentree.Service.Contracts;

namespace Talentree.Service.Services
{
    public class ImageService : IImageService
    {
        private readonly Cloudinary _cloudinary;
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB
        private readonly string[] _allowedTypes = { "image/jpeg", "image/png", "image/jpg" };

        public ImageService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public bool IsValidImage(IFormFile file)
        {
            if (file.Length > _maxFileSize) return false;
            if (!_allowedTypes.Contains(file.ContentType.ToLower())) return false;
            return true;
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folder)
        {
            using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = $"talentree/{folder}",
                Transformation = new Transformation().Quality("auto").FetchFormat("auto")
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
                throw new Exception($"Cloudinary upload failed: {result.Error.Message}");

            return result.SecureUrl.ToString();
        }

        public async Task DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            var publicId = ExtractPublicId(imageUrl);
            if (string.IsNullOrEmpty(publicId)) return;

            await _cloudinary.DestroyAsync(new DeletionParams(publicId));
        }

        /// <summary>
        /// Extracts the Cloudinary public ID from a full URL.
        /// e.g., "https://res.cloudinary.com/xxx/image/upload/v123/talentree/products/abc.jpg"
        ///   -> "talentree/products/abc"
        /// </summary>
        private static string? ExtractPublicId(string imageUrl)
        {
            // Only process Cloudinary URLs
            if (!imageUrl.Contains("cloudinary.com"))
                return null;

            try
            {
                var uri = new Uri(imageUrl);
                var path = uri.AbsolutePath; // /image/upload/v123/talentree/products/abc.jpg

                // Find the segment after "upload/vXXX/"
                var uploadIndex = path.IndexOf("/upload/");
                if (uploadIndex < 0) return null;

                var afterUpload = path[(uploadIndex + 8)..]; // v123/talentree/products/abc.jpg

                // Skip the version segment (v123/)
                var slashIndex = afterUpload.IndexOf('/');
                if (slashIndex < 0) return null;

                var publicIdWithExt = afterUpload[(slashIndex + 1)..]; // talentree/products/abc.jpg

                // Remove file extension
                var lastDot = publicIdWithExt.LastIndexOf('.');
                return lastDot > 0 ? publicIdWithExt[..lastDot] : publicIdWithExt;
            }
            catch
            {
                return null;
            }
        }
    }
}
