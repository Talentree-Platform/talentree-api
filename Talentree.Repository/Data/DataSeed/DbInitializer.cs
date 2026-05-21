// ============================================================
// Talentree.Repository/Data/DataSeed/DbInitializer.cs
// ============================================================
// Single entry point for all seed operations.
// Called exclusively from HostExtensions.MigrateDatabaseAsync
// AFTER migrations have been applied.
// Order matters — each step depends on the previous one.
// ============================================================
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Talentree.Core.Entities.Identity;
using Talentree.Repository.Data.SeedData;

namespace Talentree.Repository.Data.DataSeed
{
    public static class DbInitializer
    {
        public static async Task SeedAllAsync(
            TalentreeDbContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<AppUser> userManager,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("DbInitializer");

            // ── 1: Roles + Admin + 5 generic BOs + 5 Customers ───────────────────────
            //       (TalentreeContextSeed handles ahmed.hassan, fatma.mohamed, etc.)
            await TalentreeContextSeed.SeedAsync(userManager, roleManager, context, logger);

            // ── 2: Fixed-GUID BOs — Nour Couture, Karim Craft Studio, Salma Naturals ─
            //       Must run before any seed that references these GUIDs.
            await BusinessOwnerSeed.SeedAsync(context, userManager);

            // ── 3: Categories (Fashion, Crafts, Beauty) ──────────────────────────────
            //       Also seeds 16 enriched products with images, ratings, view counts.
            await CategoriesSeed.SeedAsync(context);

            // ── 4: Raw materials and suppliers ───────────────────────────────────────
            //       Must come before MaterialOrderSeed.
            await RawMaterialSeed.SeedAsync(context);

            // ── 5: Material orders ────────────────────────────────────────────────────
            //       References BO GUIDs from step 2 and materials from step 4.
            await MaterialOrderSeed.SeedAsync(context);

            // ── 6: Production requests ────────────────────────────────────────────────
            //       References BO GUIDs from step 2 and materials from step 4.
            await ProductionRequestSeed.SeedAsync(context);

            // ── 7: Transactions ───────────────────────────────────────────────────────
            //       Matches material orders (step 5) and production requests (step 6).
            await TransactionSeed.SeedAsync(context);

            // ── 8: Payout requests ────────────────────────────────────────────────────
            //       Requires BO accounts from step 2.
            await PayoutRequestSeed.SeedAsync(context);

            // ── 9: Product reviews ────────────────────────────────────────────────────
            //       Requires approved products (step 3) and customer accounts (step 1).
            await ProductReviewSeed.SeedAsync(context);

            // ── 10: Support tickets ───────────────────────────────────────────────────
            //        Requires BO accounts (step 2) and admin user (step 1).
            await SupportTicketSeed.SeedAsync(context);

            // ── 11: FAQs ─────────────────────────────────────────────────────────────
            await FAQSeeder.SeedFAQsAsync(context);

            logger.LogInformation("✅ All seed operations completed successfully.");
        }
    }
}