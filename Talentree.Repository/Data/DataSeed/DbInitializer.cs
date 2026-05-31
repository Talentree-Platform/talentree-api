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
using Talentree.Repository.Data;

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

            // ── 1: Roles + Admin + 6 Customers (fixed GUIDs) ─────────────────────
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

            // ── 9b: Customer features (Carts, Wishlists, Orders) ──────────────────────
            //        Requires approved products (step 3) and customer accounts (step 1).
            await CustomerFeaturesSeed.SeedAsync(context);

            // ── 10: Support tickets ───────────────────────────────────────────────────
            //        Requires BO accounts (step 2) and admin user (step 1).
            await SupportTicketSeed.SeedAsync(context);

            // ── 11: FAQs ─────────────────────────────────────────────────────────────
            await FAQSeeder.SeedFAQsAsync(context);

            // ── 12: JSON seed folder (jsonSeed/) ─────────────────────────────────────
            //        Bulk-loads Transactions, LoginHistories, ProductReviews,
            //        SupportTickets, TicketMessages, OnboardingProgress,
            //        PayoutRequests, BoProductionRequests, and Product stats
            //        from the jsonSeed/ folder alongside this assembly.
            //        All user/product FK references are validated before insert.
            //        Each section is idempotent — it checks Any() before inserting.
            var jsonSeedFolder = ResolveJsonSeedFolderPath();
            if (jsonSeedFolder != null)
                await JsonSeedLoader.SeedAsync(context, jsonSeedFolder);
            else
                logger.LogWarning("[JsonSeedLoader] jsonSeed/ folder not found — skipping bulk import.");

            logger.LogInformation("✅ All seed operations completed successfully.");
        }

        /// <summary>
        /// Resolves the path to the jsonSeed/ folder.
        /// Primary:   output directory alongside the assembly (copied on build).
        /// Fallbacks: repo source tree locations for developer convenience.
        /// </summary>
        private static string? ResolveJsonSeedFolderPath()
        {
            const string folderName = "jsonSeed";

            var candidates = new[]
            {
                // ── Primary: copied to output directory alongside the assembly ─────
                Path.Combine(AppContext.BaseDirectory, folderName),

                // ── Secondary: DataSeed folder in the repo (API runs 3 levels up) ──
                Path.GetFullPath(Path.Combine(
                    AppContext.BaseDirectory, "..", "..", "..",
                    "Talentree.Repository", "Data", "DataSeed", folderName)),

                // ── Tertiary: two levels up (inside Repository net8.0 output) ──────
                Path.GetFullPath(Path.Combine(
                    AppContext.BaseDirectory, "..", "..", "..", "..",
                    "Talentree.Repository", "Data", "DataSeed", folderName)),
            };

            return candidates.FirstOrDefault(Directory.Exists);
        }
    }
}