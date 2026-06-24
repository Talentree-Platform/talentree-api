namespace Talentree.Service.DTOs.Auth
{
    public class VerifyTwoFactorDto
    {
        public string UserId { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
    }
}
