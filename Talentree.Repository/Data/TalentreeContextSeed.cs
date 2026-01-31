using Microsoft.AspNetCore.Identity;
using Talentree.Repository.Data.DataSeed;

namespace Talentree.Repository.Data
{
    /// <summary>
    /// Main orchestrator for seeding all database tables
    /// Coordinates seeding of products, categories, brands, etc.
    /// </summary>
    public static class TalentreeContextSeed
    {
        /// <summary>
        /// Seeds all required data for development and testing
        /// </summary>
        /// <param name="context">Database context</param>
        public static async Task SeedAsync(TalentreeDbContext context)
        {
            try
            {
                // Seed products from JSON file
                //await ProductsSeed.SeedAsync(context);

               ;
            }
            catch (Exception ex)
            {
                // Log error with more context
                Console.WriteLine($"[TalentreeContextSeed] Error during data seeding: {ex.Message}");

                // Re-throw to be handled by HostExtensions
                throw;
            }
        }
    }
}