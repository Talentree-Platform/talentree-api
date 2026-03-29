namespace Talentree.Service.DTOs.AccountSettings
{
    public class LoginHistoryDto
    {
        public string IpAddress { get; set; } = string.Empty;
        public string? DeviceInfo { get; set; }
        public string? Location { get; set; }
        public DateTime LoginAt { get; set; }
        public bool IsSuccessful { get; set; }
    }
}