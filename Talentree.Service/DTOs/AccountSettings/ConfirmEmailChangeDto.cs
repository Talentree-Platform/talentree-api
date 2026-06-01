namespace Talentree.Service.DTOs.AccountSettings
{
    public class ConfirmEmailChangeDto
    {
        public string OtpCode { get; set; } = string.Empty;
        public string NewEmail { get; set; } = string.Empty;
    }
}
