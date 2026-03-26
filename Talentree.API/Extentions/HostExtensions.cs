// ============================================================
// Talentree.API/Extensions/HostExtensions.cs
// ============================================================
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Talentree.Core.Entities;
using Talentree.Repository.Data;
using Talentree.Repository.Data.DataSeed;

namespace Talentree.API.Extensions
{
    public static class HostExtensions
    {
        /// <summary>
        /// Applies pending migrations then runs all seed operations in the correct order.
        /// This is the ONLY place seeding should be triggered.
        /// </summary>
        public static async Task<IHost> MigrateDatabaseAsync(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("DatabaseMigration");

            try
            {
                var dbContext = services.GetRequiredService<TalentreeDbContext>();
                var userManager = services.GetRequiredService<UserManager<AppUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                // 1 — Apply any pending EF Core migrations first
                logger.LogInformation("Applying migrations...");
                await dbContext.Database.MigrateAsync();
                logger.LogInformation("Migrations applied successfully.");

                // 2 — Run all seed operations via the single DbInitializer
                logger.LogInformation("Starting data seeding...");
                await DbInitializer.SeedAllAsync(dbContext, roleManager, userManager, loggerFactory);
                logger.LogInformation("Data seeding completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during database migration or seeding.");
                throw;
            }

            return host;
        }
    }
}