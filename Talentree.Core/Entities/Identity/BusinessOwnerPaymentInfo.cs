using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talentree.Core.Entities.Identity
{
    public class BusinessOwnerPaymentInfo : BaseEntity
    {
        // FK
        public string UserId { get; set; } = null!;
        public AppUser User { get; set; } = null!;

        // Bank details (stored encrypted)
        public string BankName { get; set; } = null!;
        public string AccountHolderName { get; set; } = null!;
        public string AccountNumberEncrypted { get; set; } = null!; // AES encrypted
        public string RoutingSwiftCode { get; set; } = null!;

        // Audit
        public DateTime? LastChangedAt { get; set; }
        public string? LastChangedByIp { get; set; }
    }
}
