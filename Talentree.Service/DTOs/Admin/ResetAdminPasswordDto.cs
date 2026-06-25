namespace Talentree.Service.DTOs.Admin
{
    public class ResetAdminPasswordDto
    {
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
