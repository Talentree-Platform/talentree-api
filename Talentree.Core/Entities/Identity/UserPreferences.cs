using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talentree.Core.Entities.Identity
{
    public class UserPreferences : BaseEntity
    {
        public string UserId { get; set; } = null!;
        public AppUser User { get; set; } = null!;

        public string Timezone { get; set; } = "Africa/Cairo";
        public string DateFormat { get; set; } = "dd/MM/yyyy";
        public string CurrencyDisplay { get; set; } = "EGP";
        public string DashboardLayout { get; set; } = "default"; // "default", "compact", "detailed"
        // Language is Phase 2 — leave null for now
        public string? Language { get; set; }
    }
}
