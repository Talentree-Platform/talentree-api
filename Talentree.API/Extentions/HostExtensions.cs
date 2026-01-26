using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Talentree.Core.Entities.Identity;
using Talentree.Repository.Data;
using Talentree.Repository.Identity;

namespace Talentree.API.Extensions
{
    /// <summary>
    /// Extension methods for IHost to handle database initialization
    /// </summary>
    public static class HostExtensions
    {
        /// <summary>
        /// Applies pending migrations and seeds initial data
        /// Should only be used in Development environment
        /// </summary>
        /// <param name="host">The application host</param>
        /// <returns>The host for chaining</returns>
        public static async Task<IHost> MigrateDatabaseAsync(this IHost host)
        {
            // Create a scope to resolve scoped services
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            // Get logger factory for error logging
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("DatabaseMigration");

            try
            {
                // ===============================
                // Migrate Main Database (Store)
                // ===============================
                logger.LogInformation("Starting migration for TalentreeDbContext...");

                var dbContext = services.GetRequiredService<TalentreeDbContext>();

                // Apply any pending migrations
                await dbContext.Database.MigrateAsync();

                logger.LogInformation("TalentreeDbContext migration completed successfully.");

                // ===============================
                // Seed Initial Data
                // ===============================
                logger.LogInformation("Starting data seeding...");

                await TalentreeContextSeed.SeedAsync(dbContext);

                logger.LogInformation("Data seeding completed successfully.");

                // ===============================
                // 3️⃣ Migrate Identity Database
                // ===============================
                await MigrateIdentityDatabaseAsync(services, logger);
            }
            catch (Exception ex)
            {
                // Log the error with full stack trace
                logger.LogError(ex, "An error occurred during database migration.");

                // In development, we might want to see the error
                // In production, we might want to continue without crashing
                throw; // Re-throw to prevent app startup with invalid database
            }

            return host;
        }
        /// <summary>
        /// Migrates Identity Database and seeds default users
        /// </summary>
        private static async Task MigrateIdentityDatabaseAsync(
            IServiceProvider services,
            ILogger logger)
        {
            try
            {
                logger.LogInformation("Starting migration for AppIdentityDbContext...");

                // Get Identity DbContext
                var identityDbContext = services.GetRequiredService<AppIdentityDbContext>();

                // Apply pending migrations
                await identityDbContext.Database.MigrateAsync();
                logger.LogInformation("AppIdentityDbContext migration completed successfully.");

                // ===============================
                //  Seed Identity Data
                // ===============================
                // Uncomment when you create AppIdentityDbContextSeed class
                
                logger.LogInformation("Starting identity data seeding...");
                
                var userManager = services.GetRequiredService<UserManager<AppUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                
                await AppIdentityDbContextSeed.SeedAsync(userManager, roleManager, logger);
                
                logger.LogInformation("Identity data seeding completed successfully.");
                
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during Identity database migration.");
                throw;
            }
        }
    }
}