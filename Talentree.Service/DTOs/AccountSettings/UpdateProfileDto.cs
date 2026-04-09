namespace Talentree.Service.DTOs.AccountSettings
{
    public class UpdateProfileDto
    {
        // Personal
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? NewEmail { get; set; } // if changed → triggers OTP

        // Business
        public string BusinessName { get; set; } = string.Empty;
        public string BusinessDescription { get; set; } = string.Empty;
        public string? BusinessAddress { get; set; }

        // Contact
        public string? FacebookLink { get; set; }
        public string? InstagramLink { get; set; }
        public string? WebsiteLink { get; set; }

        // Photos via IFormFile in controller
    }
}