namespace Talentree.Service.DTOs.AccountSettings
{
    public class ProfileDto
    {
        // Personal
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? ProfilePhotoUrl { get; set; }

        // Business
        public string BusinessName { get; set; } = string.Empty;
        public string BusinessDescription { get; set; } = string.Empty;
        public string? BusinessAddress { get; set; }
        public string? BusinessLogoUrl { get; set; }
        public string BusinessStatus { get; set; } = string.Empty;

        // Contact
        public string? FacebookLink { get; set; }
        public string? InstagramLink { get; set; }
        public string? WebsiteLink { get; set; }
    }
}