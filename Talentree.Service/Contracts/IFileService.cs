using Microsoft.AspNetCore.Http;

namespace Talentree.Service.Contracts
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(IFormFile file, string folder);
        Task<bool> DeleteFileAsync(string fileUrl);
        string GetFileSizeMB(long bytes);
    }
}