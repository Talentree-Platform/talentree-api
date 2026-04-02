using Microsoft.AspNetCore.Http;

namespace Talentree.Service.DTOs.AccountSettings
{
    public class UpdateProfileWithPhotosDto : UpdateProfileDto
    {
        public IFormFile? ProfilePhoto { get; set; }
        public IFormFile? BusinessLogo { get; set; }
    }
}