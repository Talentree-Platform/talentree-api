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
            //       Guards with Any() so it only runs on a fresh DB — backup JSON
            //       (step 12) then adds historical records starting from higher IDs.
            await TransactionSeed.SeedAsync(context);

            // ── 8: Payout requests ────────────────────────────────────────────────────
            //       Requires BO accounts from step 2.
            //       Same guard pattern as transactions above.
            await PayoutRequestSeed.SeedAsync(context);

            // ── 9: Product reviews ────────────────────────────────────────────────────
            //       Requires approved products (step 3) and customer accounts (step 1).
            await ProductReviewSeed.SeedAsync(context);

            // ── 10: Support tickets ───────────────────────────────────────────────────
            //        Requires BO accounts (step 2) and admin user (step 1).
            await SupportTicketSeed.SeedAsync(context);

            // ── 11: FAQs ─────────────────────────────────────────────────────────────
            await FAQSeeder.SeedFAQsAsync(context);

            // ── 12: JSON Backup (seeded_data_backup.json) ─────────────────────────────
            //        Bulk-loads historical Transactions, LoginHistories, ProductReviews,
            //        SupportTickets, TicketMessages, OnboardingProgress, PayoutRequests,
            //        and BoProductionRequests from the exported backup file.
            //
            //        The file lives in DataSeed/ and is copied to the output directory
            //        on build so it works on every machine without manual file placement.
            //
            //        Each section is idempotent — it skips insertion if its first ID
            //        already exists, so re-running the seeder is safe.
            var jsonBackupPath = ResolveBackupJsonPath();
            if (jsonBackupPath != null)
                await JsonBackupSeed.SeedAsync(context, jsonBackupPath);
            else
                logger.LogWarning("[JsonBackupSeed] seeded_data_backup.json not found — skipping bulk import.");

            logger.LogInformation("✅ All seed operations completed successfully.");
        }

        /// <summary>
        /// Resolves the path to seeded_data_backup.json.
        /// Primary location: DataSeed/ folder next to the assembly (copied on build).
        /// Fallback: repo root and Downloads folder for developer convenience.
        /// </summary>
        private static string? ResolveBackupJsonPath()
        {
            const string fileName = "seeded_data_backup.json";

            var candidates = new[]
            {
                // ── Primary: copied to output directory alongside the assembly ─────
                // (requires <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
                //  on the .json file in the .csproj — see Talentree.Repository.csproj)
                Path.Combine(AppContext.BaseDirectory, fileName),

                // ── Secondary: DataSeed folder relative to the repo root ───────────
                // Resolves correctly when running from the API project (3 levels up).
                Path.GetFullPath(Path.Combine(
                    AppContext.BaseDirectory, "..", "..", "..",
                    "Talentree.Repository", "Data", "DataSeed", fileName)),

                // ── Tertiary: two levels up from net8.0/ inside Repository project ─
                Path.GetFullPath(Path.Combine(
                    AppContext.BaseDirectory, "..", "..", "..", "..",
                    "Talentree.Repository", "Data", "DataSeed", fileName)),

                // ── Developer fallback: Downloads folder ──────────────────────────
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Downloads", fileName),
            };

            return candidates.FirstOrDefault(File.Exists);
        }
    }
}