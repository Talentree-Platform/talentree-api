using System.Reflection;
using System.Text.Json;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.DataSeed
{
    /// <summary>
    /// Seeds products data from JSON file
    /// </summary>
    public static class ProductsSeed
    {
        /// <summary>
        /// Seeds products if the Products table is empty
        /// </summary>
        /// <param name="context">Database context</param>
        public static async Task SeedAsync(TalentreeDbContext context)
        {
            // Check if products already exist
            if (context.Products.Any())
            {
                Console.WriteLine("[ProductsSeed] Products already exist, skipping seed.");
                return;
            }

            try
            {
                // Get the path to the JSON file relative to the assembly
                // This ensures it works regardless of working directory
                var filePath = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                    "Data",
                    "DataSeed",
                    "products.json"
                );

                // Check if file exists
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"[ProductsSeed] WARNING: products.json not found at: {filePath}");
                    return;
                }

                Console.WriteLine($"[ProductsSeed] Reading products from: {filePath}");

                // Read and deserialize JSON
                var jsonData = await File.ReadAllTextAsync(filePath);
                var products = JsonSerializer.Deserialize<List<Product>>(jsonData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // Handle case differences
                });

                // Validate data
                if (products == null || !products.Any())
                {
                    Console.WriteLine("[ProductsSeed] No products found in JSON file.");
                    return;
                }

                Console.WriteLine($"[ProductsSeed] Found {products.Count} products to seed.");

                // Add to database
                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();

                Console.WriteLine($"[ProductsSeed] Successfully seeded {products.Count} products.");
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"[ProductsSeed] JSON parsing error: {jsonEx.Message}");
                throw new Exception("Failed to parse products.json. Please check the JSON format.", jsonEx);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProductsSeed] Error seeding products: {ex.Message}");
                throw;
            }
        }
    }
}
