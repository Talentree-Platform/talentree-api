// ============================================================
// Talentree.Repository/Data/DataSeed/DbInitializer.cs
// ============================================================
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Talentree.Core.Entities;
using Talentree.Core.Entities.Identity;

namespace Talentree.Repository.Data.DataSeed
{
    /// <summary>
    /// Single entry point for all seed operations.
    /// Called exclusively from HostExtensions.MigrateDatabaseAsync
    /// AFTER migrations have been applied.
    /// Order matters — each step depends on the previous one.
    /// </summary>
    public static class DbInitializer
    {
        public static async Task SeedAllAsync(
            TalentreeDbContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<AppUser> userManager,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("DbInitializer");

            // 1 — Roles + Admin + Customers (TalentreeContextSeed handles all of these)
            //     Also seeds its own set of BOs (ahmed.hassan etc.)
            await TalentreeContextSeed.SeedAsync(userManager, roleManager, context, logger);

            // 2 — Our additional seeded BOs with fixed GUIDs (nour.elsayed, karim.mansour, salma.tarek)
            //     These are needed for MaterialOrderSeed and ProductionRequestSeed
            await BusinessOwnerSeed.SeedAsync(context, userManager);

            // 3 — Categories
            await CategoriesSeed.SeedAsync(context);

            // 4 — Raw materials and suppliers
            //     Must come before MaterialOrderSeed
            await RawMaterialSeed.SeedAsync(context);

            // 5 — Material orders (references BO GUIDs from step 2 + materials from step 4)
            await MaterialOrderSeed.SeedAsync(context);

            // 6 — Production requests (references BO GUIDs from step 2 + materials from step 4)
            await ProductionRequestSeed.SeedAsync(context);

            logger.LogInformation("✅ All seed operations completed successfully.");
        }
    }
}