using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talentree.Core.Entities.Identity
{
    public class RefreshToken : BaseEntity
    {
        public string TokenHash { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }

        public string? ReplacedByTokenHash { get; set; }

        // FK
        public string UserId { get; set; } = null!;
        public AppUser User { get; set; } = null!;

        // Computed
        public bool IsActive =>
            RevokedAt == null && DateTime.UtcNow < ExpiresAt;
    }

}
