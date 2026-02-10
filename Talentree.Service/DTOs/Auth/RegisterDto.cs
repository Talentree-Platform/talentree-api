// Talentree.Service/DTOs/Auth/RegisterDto.cs

namespace Talentree.Service.DTOs.Auth
{
    /// <summary>
    /// DTO for user registration
    /// </summary>
    public class RegisterDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        /// <summary>
        /// User's email address (will be used as username)
        /// </summary>
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// Role to assign: "Customer" or "BusinessOwner"
        /// Admin cannot register through this endpoint
        /// </summary>
        public string Role { get; set; } = "Customer";
    }
}