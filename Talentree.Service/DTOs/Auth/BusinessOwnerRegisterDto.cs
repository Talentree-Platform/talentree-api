
namespace Talentree.Service.DTOs.Auth
{
    
    public class BusinessOwnerRegisterDto
    {
        // ═══════════════════════════════════════════════════════════
        // Personal Information 
        // ═══════════════════════════════════════════════════════════

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string ConfirmPassword { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        // ═══════════════════════════════════════════════════════════
        // Business Information (Required)
        // ═══════════════════════════════════════════════════════════

        public string BusinessName { get; set; } = string.Empty;

        public string BusinessCategory { get; set; } = string.Empty;

        public string BusinessDescription { get; set; } = string.Empty;

        // ═══════════════════════════════════════════════════════════
        // Optional Business Information (MVP)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Business physical address (optional for MVP)
        /// </summary>
        public string? BusinessAddress { get; set; }

        /// <summary>
        /// Tax ID / Commercial Registration (optional for MVP)
        /// </summary>
        public string? TaxId { get; set; }

        public string? FacebookLink { get; set; }

        public string? InstagramLink { get; set; }
        public string? WebsiteLink { get; set; }
    }
}