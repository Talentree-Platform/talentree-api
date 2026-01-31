// Talentree.Service/DTOs/Auth/LoginDto.cs

namespace Talentree.Service.DTOs.Auth
{
    /// <summary>
    /// DTO for user login
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// User's email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User's password
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }
}