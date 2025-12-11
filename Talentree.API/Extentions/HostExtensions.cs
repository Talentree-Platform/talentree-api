using Microsoft.EntityFrameworkCore;
using Talentree.Repository.Data;

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

                // TODO: Add Identity migration when ready
                // await MigrateIdentityDatabaseAsync(services, logger);
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
        /// Migrates Identity Database (to be implemented later)
        /// </summary>
        private static async Task MigrateIdentityDatabaseAsync(
            IServiceProvider services,
            ILogger logger)
        {
            // TODO: Implement when Identity is added
            /*
            logger.LogInformation("Starting migration for AppIdentityDbContext...");
            
            var identityDbContext = services.GetRequiredService<AppIdentityDbContext>();
            await identityDbContext.Database.MigrateAsync();
            
            logger.LogInformation("AppIdentityDbContext migration completed.");
            
            // Seed Identity Users
            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            await AppIdentityDbContextSeed.SeedUsersAsync(userManager);
            */
        }
    }
}