// ============================================================
// Talentree.Repository/Data/DataSeed/PlatformPolicySeed.cs
// ============================================================
// Seeds one published v1 row for each PolicyDocumentType.
// Idempotent — skips insert if any PlatformPolicy rows already exist.
// ============================================================
using Microsoft.EntityFrameworkCore;
using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Repository.Data.DataSeed
{
    public static class PlatformPolicySeed
    {
        public static async Task SeedAsync(TalentreeDbContext context)
        {
            // Idempotency guard — if any rows already exist, skip entirely
            if (await context.PlatformPolicies.AnyAsync())
                return;

            var seedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            const string seededBy = "system-seed";

            var policies = new List<PlatformPolicy>
            {
                new()
                {
                    DocumentType       = PolicyDocumentType.TermsOfService,
                    Content            = "<h1>Terms of Service</h1><p>Welcome to Talentree. By using our platform you agree to these terms. " +
                                         "These terms govern your use of the Talentree marketplace, including browsing, purchasing, " +
                                         "and selling. We reserve the right to modify these terms at any time. " +
                                         "Continued use of the platform constitutes acceptance of the updated terms.</p>",
                    VersionNumber      = 1,
                    IsPublished        = true,
                    RequireUserAcceptance = true,
                    PublishedAt        = seedDate,
                    CreatedAt          = seedDate,
                    CreatedBy          = seededBy
                },
                new()
                {
                    DocumentType       = PolicyDocumentType.PrivacyPolicy,
                    Content            = "<h1>Privacy Policy</h1><p>Talentree is committed to protecting your personal data. " +
                                         "We collect only the information necessary to provide our services. " +
                                         "Your data is never sold to third parties. " +
                                         "You may request deletion of your data at any time by contacting our support team.</p>",
                    VersionNumber      = 1,
                    IsPublished        = true,
                    RequireUserAcceptance = true,
                    PublishedAt        = seedDate,
                    CreatedAt          = seedDate,
                    CreatedBy          = seededBy
                },
                new()
                {
                    DocumentType       = PolicyDocumentType.ReturnAndRefundPolicy,
                    Content            = "<h1>Return & Refund Policy</h1><p>Customers may request a return within 14 days of receiving their order. " +
                                         "Items must be in their original condition and packaging. " +
                                         "Refunds are processed within 5-7 business days after the returned item is received and inspected. " +
                                         "Digital products and custom orders are non-refundable.</p>",
                    VersionNumber      = 1,
                    IsPublished        = true,
                    RequireUserAcceptance = false,
                    PublishedAt        = seedDate,
                    CreatedAt          = seedDate,
                    CreatedBy          = seededBy
                },
                new()
                {
                    DocumentType       = PolicyDocumentType.SellerAgreement,
                    Content            = "<h1>Seller Agreement</h1><p>By registering as a Business Owner on Talentree, you agree to maintain " +
                                         "accurate product listings, fulfill orders in a timely manner, and comply with all applicable laws. " +
                                         "Talentree charges a platform commission on each completed sale as outlined in the fee schedule. " +
                                         "Violation of these terms may result in account suspension.</p>",
                    VersionNumber      = 1,
                    IsPublished        = true,
                    RequireUserAcceptance = true,
                    PublishedAt        = seedDate,
                    CreatedAt          = seedDate,
                    CreatedBy          = seededBy
                },
                new()
                {
                    DocumentType       = PolicyDocumentType.CommunityGuidelines,
                    Content            = "<h1>Community Guidelines</h1><p>Talentree is a respectful marketplace for artisans and customers alike. " +
                                         "Harassment, hate speech, fraudulent listings, and counterfeit goods are strictly prohibited. " +
                                         "Users who violate these guidelines will be warned and may be permanently banned. " +
                                         "Please report any violations using the in-app report feature.</p>",
                    VersionNumber      = 1,
                    IsPublished        = true,
                    RequireUserAcceptance = false,
                    PublishedAt        = seedDate,
                    CreatedAt          = seedDate,
                    CreatedBy          = seededBy
                }
            };

            await context.PlatformPolicies.AddRangeAsync(policies);
            await context.SaveChangesAsync();

            Console.WriteLine("[PlatformPolicySeed] ✅ Seeded 5 platform policy documents.");
        }
    }
}
