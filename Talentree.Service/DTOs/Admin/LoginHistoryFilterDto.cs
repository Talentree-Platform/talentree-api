using System;

namespace Talentree.Service.DTOs.Admin
{
    public class LoginHistoryFilterDto
    {
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public bool? IsSuccessful { get; set; }
        public string? IpAddress { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
