using System;

namespace Talentree.Service.DTOs.Admin
{
    public class UserActionLogFilterDto
    {
        public string? AdminId { get; set; }
        public string? Action { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
