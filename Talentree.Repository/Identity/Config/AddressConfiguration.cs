using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities.Identity;
using Talentree.Repository.Identity.Config;

namespace Talentree.Repository.Identity.Config
{
    /// <summary>
    /// Entity Framework configuration for Address entity
    /// Defines table structure, constraints, and relationships using Fluent API
    /// </summary>
    /// <remarks>
    /// Why Fluent API instead of Data Annotations?
    /// 
    /// Advantages:
    /// 1. Separation: Keeps domain entities clean (no EF attributes)
    /// 2. Flexibility: More configuration options available
    /// 3. Centralization: All DB config in one place
    /// 4. Testability: Easier to test domain entities
    /// 5. Readability: Clear and explicit configurations
    /// </remarks>
    public class AddressConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
            // ===============================
            // Table Name
            // ===============================

            // Explicitly set table name for clarity
            // Default would be "Addresses" anyway, but being explicit is better
            builder.ToTable("Addresses");

            // ===============================
            // Primary Key
            // ===============================

            // Id is already configured by BaseEntity
            // EF Core automatically detects property named "Id" as primary key
            // But we can be explicit for documentation:
            builder.HasKey(a => a.Id);

            // ===============================
            // Required Fields (NOT NULL)
            // ===============================

            /// <summary>
            /// FirstName: Required for shipping label
            /// MaxLength: 100 characters (reasonable for names)
            /// </summary>
            builder.Property(a => a.FirstName)
                .IsRequired()                // NOT NULL constraint
                .HasMaxLength(100);          // VARCHAR(100)

            /// <summary>
            /// LastName: Required for shipping label
            /// MaxLength: 100 characters
            /// </summary>
            builder.Property(a => a.LastName)
                .IsRequired()
                .HasMaxLength(100);

            /// <summary>
            /// Street: Required (core address info)
            /// MaxLength: 200 characters (enough for "123 Main St, Apt 4B")
            /// </summary>
            builder.Property(a => a.Street)
                .IsRequired()
                .HasMaxLength(200);

            /// <summary>
            /// City: Required for delivery
            /// MaxLength: 100 characters
            /// </summary>
            builder.Property(a => a.City)
                .IsRequired()
                .HasMaxLength(100);

            /// <summary>
            /// State/Governorate: Required
            /// MaxLength: 100 characters (e.g., "California", "Cairo Governorate")
            /// </summary>
            builder.Property(a => a.State)
                .IsRequired()
                .HasMaxLength(100);

            /// <summary>
            /// ZipCode: Required for postal service
            /// MaxLength: 20 characters (handles international formats)
            /// Example: "12345", "12345-6789", "SW1A 1AA" (UK)
            /// </summary>
            builder.Property(a => a.ZipCode)
                .IsRequired()
                .HasMaxLength(20);

            /// <summary>
            /// Country: Required for international shipping
            /// MaxLength: 100 characters
            /// Consider: ISO country codes (2-letter) for standardization
            /// </summary>
            builder.Property(a => a.Country)
                .IsRequired()
                .HasMaxLength(100);

            // ===============================
            // Foreign Key & Relationship
            // ===============================

            /// <summary>
            /// AppUserId: Foreign key to AppUser
            /// Required because every address must belong to a user
            /// </summary>
            builder.Property(a => a.AppUserId)
                .IsRequired();                // NOT NULL

            /// <summary>
            /// One-to-One relationship: AppUser → Address
            /// 
            /// Configuration:
            /// - One user (AppUser) has one address (Address)
            /// - Address.AppUserId is the foreign key
            /// - OnDelete: Cascade (if user deleted, address deleted too)
            /// 
            /// Why Cascade?
            /// - Address has no meaning without user
            /// - Prevents orphaned address records
            /// - Automatic cleanup
            /// </summary>
            builder.HasOne(a => a.AppUser)              // Address has one AppUser
                .WithOne(u => u.Address)                // AppUser has one Address
                .HasForeignKey<Address>(a => a.AppUserId)  // FK is Address.AppUserId
                .OnDelete(DeleteBehavior.Cascade);      // Delete address if user deleted

            // ===============================
            // Indexes (Optional but recommended)
            // ===============================

            // Index on AppUserId for faster lookups
            // When querying: "Get address for user X"
            builder.HasIndex(a => a.AppUserId)
                .HasDatabaseName("IX_Addresses_AppUserId");

            // Note: No unique constraint on AppUserId because HasOne/WithOne already implies uniqueness
        }
    }
}