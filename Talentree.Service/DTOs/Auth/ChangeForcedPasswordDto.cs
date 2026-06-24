namespace Talentree.Service.DTOs.Auth
{
    public class ChangeForcedPasswordDto
    {
        public string UserId { get; set; } = string.Empty;
        public string TemporaryPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
