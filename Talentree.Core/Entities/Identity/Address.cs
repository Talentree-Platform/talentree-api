using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.IO;
using Talentree.Core.Entities.Identity;

namespace Talentree.Core.Entities.Identity
{
    /// <summary>
    /// Represents a physical address for shipping or billing
    /// </summary>
    /// <remarks>
    /// Why separate Address entity instead of fields in AppUser?
    /// 
    /// Advantages:
    /// 1. Optional: Not all users need address immediately
    /// 2. Extensible: Easy to add multiple addresses later
    /// 3. Reusable: Same address format for different purposes
    /// 4. Validation: Can validate address fields separately
    /// 5. Query optimization: Can load users without addresses
    /// </remarks>
    public class Address : BaseEntity
    {
        /// <summary>
        /// First name for shipping label
        /// </summary>
        /// <remarks>
        /// Separate from AppUser.DisplayName because:
        /// - Display name might be nickname/company name
        /// - Shipping needs legal first name
        /// - Different formats for different purposes
        /// </remarks>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Last name for shipping label
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Street address (house number + street name)
        /// Example: "123 Main Street, Apt 4B"
        /// </summary>
        public string Street { get; set; } = string.Empty;

        /// <summary>
        /// City name
        /// Example: "Cairo", "Alexandria"
        /// </summary>
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// State or Governorate
        /// Example: "Cairo Governorate", "California"
        /// </summary>
        public string State { get; set; } = string.Empty;

        /// <summary>
        /// Postal/ZIP code for delivery
        /// Example: "11511", "90210"
        /// </summary>
        public string ZipCode { get; set; } = string.Empty;

        /// <summary>
        /// Country name or ISO code
        /// Example: "Egypt", "United States"
        /// </summary>
        /// <remarks>
        /// Consider using:
        /// - ISO 3166-1 alpha-2 codes (EG, US)
        /// - Dropdown with predefined countries
        /// - For international shipping support
        /// </remarks>
        public string Country { get; set; } = string.Empty;

        // ===============================
        // Foreign Key & Navigation
        // ===============================

        /// <summary>
        /// Foreign key to the user who owns this address
        /// </summary>
        /// <remarks>
        /// Why string not int?
        /// - IdentityUser uses string as primary key (GUID by default)
        /// - Ensures FK matches PK type
        /// </remarks>
        public string AppUserId { get; set; } = string.Empty;

        /// <summary>
        /// Navigation property back to the user
        /// </summary>
        /// <remarks>
        /// Required for EF Core to establish relationship:
        /// - One user has one address
        /// - One address belongs to one user
        /// - = One-to-One relationship
        /// </remarks>
        public AppUser AppUser { get; set; } = null!;
    }
}
