using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Talentree.Repository.Data;
using Talentree.Repository.Data.DataSeed;

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
                var userManager = services.GetRequiredService<UserManager<AppUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                logger.LogInformation("Starting data seeding...");

                await TalentreeContextSeed.SeedAsync(userManager , roleManager , dbContext, logger);

                logger.LogInformation("Data seeding completed successfully.");

                await TalentreeContextSeed.SeedAsync(userManager, roleManager, dbContext, logger);
                logger.LogInformation("Data seeding completed successfully.");

                // ADD THIS
                await CategoriesSeed.SeedAsync(dbContext);
                logger.LogInformation("Categories seeding completed successfully.");

            }
            catch (Exception ex)
            {
                // Log the error with full stack trace
                logger.LogError(ex, "An error occurred during database migration.");

                
                throw; 
            }

            return host;
        }

      
    }
}