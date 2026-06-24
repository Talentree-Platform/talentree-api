using System;

namespace Talentree.Service.DTOs.Admin
{
    public class AdminLoginHistoryDto
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? UserEmail { get; set; }
        public string? UserDisplayName { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string? DeviceInfo { get; set; }
        public string? Location { get; set; }
        public DateTime LoginAt { get; set; }
        public bool IsSuccessful { get; set; }
        public string? Status { get; set; }
        public string? FailureReason { get; set; }
        public string? UserAgent { get; set; }
        public string? Device { get; set; }
    }
}
