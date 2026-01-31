// Talentree.Service/DTOs/Auth/ExternalLoginDto.cs

namespace Talentree.Service.DTOs.Auth
{
    /// <summary>
    /// DTO for external authentication (Google/Facebook)
    /// </summary>
    public class ExternalLoginDto
    {
        /// <summary>
        /// ID token from Google or access token from Facebook
        /// Google: idToken from Google Sign-In
        /// Facebook: access token from Facebook Login
        /// </summary>
        public string IdToken { get; set; } = string.Empty;

        /// <summary>
        /// Provider name: "Google" or "Facebook"
        /// </summary>
        public string Provider { get; set; } = string.Empty;
    }
}