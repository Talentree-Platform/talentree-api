// Talentree.Core/Entities/Identity/Address.cs

using Talentree.Core.Entities;

namespace Talentree.Core.Entities.Identity
{
    public class Address : AuditableEntity
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;

        public string Street { get; set; } = null!;
        public string City { get; set; } = null!;
        public string State { get; set; } = null!;
        public string ZipCode { get; set; } = null!;
        public string Country { get; set; } = null!;

        // FK
        public string UserId { get; set; } = null!;
        public AppUser User { get; set; } = null!;
    }
}
