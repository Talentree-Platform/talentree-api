using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talentree.Core.Entities.Identity
{
    public class LoginHistory : BaseEntity
    {
        public string? UserId { get; set; }
        public AppUser? User { get; set; }
        public string IpAddress { get; set; } = null!;
        public string? DeviceInfo { get; set; }
        public string? Location { get; set; }
        public DateTime LoginAt { get; set; } = DateTime.UtcNow;
        public bool IsSuccessful { get; set; } = true;
        public string? Status { get; set; }
        public string? FailureReason { get; set; }
        public string? UserAgent { get; set; }
        public string? Device { get; set; }
    }
}
