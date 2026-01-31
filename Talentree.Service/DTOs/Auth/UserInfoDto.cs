// Talentree.Service/DTOs/Auth/UserInfoDto.cs

namespace Talentree.Service.DTOs.Auth
{
    /// <summary>
    /// User information returned in authentication responses
    /// </summary>
    public class UserInfoDto
    {
        /// <summary>
        /// User's unique identifier
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// User's display name
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// User's email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User's phone number
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// User's roles (Admin, BusinessOwner, Customer)
        /// </summary>
        public List<string> Roles { get; set; } = new();

        /// <summary>
        /// Whether email is verified
        /// </summary>
        public bool IsEmailVerified { get; set; }

        /// <summary>
        /// Whether account is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// When user account was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}