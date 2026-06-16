using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Repository.Data.DataSeed
{
    public static class ProductSeeder
    {
        public static async Task<SeederResult<int>> SeedAsync(TalentreeDbContext context, string jsonSeedFolderPath)
        {
            var result = new SeederResult<int>();

            var fileName = "Products.json";
            var filePath = Path.Combine(jsonSeedFolderPath, fileName);
            if (!File.Exists(filePath))
            {
                filePath = Path.Combine(jsonSeedFolderPath, "products_db_seeding.json");
            }

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"[ProductSeeder] Seed file not found (tried Products.json and products_db_seeding.json) — skipping.");
                return result;
            }

            Console.WriteLine($"[ProductSeeder] Reading data from {filePath}...");

            var jsonText = await File.ReadAllTextAsync(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var dtos = JsonSerializer.Deserialize<List<ProductSeedDto>>(jsonText, options);
            if (dtos == null || dtos.Count == 0)
            {
                Console.WriteLine("[ProductSeeder] No products found in JSON file.");
                return result;
            }

            // 1. Validate foreign keys fail-fast
            var validBoIds = await context.BusinessOwnerProfiles.Select(b => b.Id).ToListAsync();
            var validBoIdsHash = new HashSet<int>(validBoIds);

            var validCategoryIds = await context.Categories.Select(c => c.Id).ToListAsync();
            var validCategoryIdsHash = new HashSet<int>(validCategoryIds);

            // 2. Load existing products to prevent duplicates and populate existing maps
            var existingProducts = await context.Products
                .Select(p => new { p.Id, p.Name })
                .ToListAsync();

            var dbProductMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in existingProducts)
            {
                if (!dbProductMap.ContainsKey(p.Name))
                {
                    dbProductMap[p.Name] = p.Id;
                }
            }

            var newProducts = new List<Product>();
            var localProductMap = new Dictionary<string, Product>(StringComparer.OrdinalIgnoreCase);
            var pendingMappings = new List<(int JsonId, Product Entity)>();

            foreach (var dto in dtos)
            {
                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    continue;
                }

                // Foreign Key validation
                if (!validBoIdsHash.Contains(dto.BusinessOwnerProfileId))
                {
                    throw new InvalidOperationException(
                        $"Foreign key validation failed: BusinessOwnerProfileId '{dto.BusinessOwnerProfileId}' does not exist in the database for product '{dto.Name}'.");
                }

                if (!validCategoryIdsHash.Contains(dto.CategoryId))
                {
                    throw new InvalidOperationException(
                        $"Foreign key validation failed: CategoryId '{dto.CategoryId}' does not exist in the database for product '{dto.Name}'.");
                }

                // Uniqueness check (Case-insensitive)
                if (dbProductMap.TryGetValue(dto.Name, out var existingDbId))
                {
                    if (dto.Id.HasValue)
                    {
                        result.IdMap[dto.Id.Value] = existingDbId;
                    }
                    continue;
                }

                // Handle duplicates within the JSON file
                if (localProductMap.TryGetValue(dto.Name, out var locallyMappedProduct))
                {
                    if (dto.Id.HasValue)
                    {
                        pendingMappings.Add((dto.Id.Value, locallyMappedProduct));
                    }
                    continue;
                }

                // Map DTO to Entity and populate default values when missing
                var product = new Product
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    Price = dto.Price,
                    StockQuantity = dto.StockQuantity,
                    Tags = dto.Tags,

                    Status = dto.Status.HasValue ? (ProductStatus)dto.Status.Value : ProductStatus.Approved,
                    RejectionReason = dto.RejectionReason,
                    ApprovedBy = dto.ApprovedBy ?? "SystemSeeder",
                    ApprovedAt = dto.ApprovedAt ?? DateTime.UtcNow,

                    ViewCount = dto.ViewCount ?? 0,
                    CartAddCount = dto.CartAddCount ?? 0,
                    PurchaseCount = dto.PurchaseCount ?? 0,
                    AvgRating = dto.AvgRating,
                    RevenueTotal = dto.RevenueTotal ?? 0m,
                    DemandForecastQty = dto.DemandForecastQty,
                    DemandForecastUpdatedAt = dto.DemandForecastUpdatedAt,
                    LowStockFlag = dto.LowStockFlag ?? false,
                    DescriptionQualityScore = dto.DescriptionQualityScore,

                    BusinessOwnerProfileId = dto.BusinessOwnerProfileId,
                    CategoryId = dto.CategoryId,

                    IsDeleted = dto.IsDeleted ?? false,
                    DeletedAt = dto.DeletedAt,
                    DeletedBy = dto.DeletedBy,

                    IsVisible = dto.IsVisible ?? true,
                    IsFeatured = dto.IsFeatured ?? false,
                    FeaturedOrder = dto.FeaturedOrder ?? 0,

                    CreatedAt = dto.CreatedAt ?? DateTime.UtcNow,
                    UpdatedAt = dto.UpdatedAt,
                    CreatedBy = dto.CreatedBy,
                    UpdatedBy = dto.UpdatedBy
                };

                newProducts.Add(product);
                localProductMap[dto.Name] = product;

                if (dto.Id.HasValue)
                {
                    pendingMappings.Add((dto.Id.Value, product));
                }
            }

            if (newProducts.Count > 0)
            {
                await context.Products.AddRangeAsync(newProducts);
                await context.SaveChangesAsync();
                result.InsertedCount = newProducts.Count;
                Console.WriteLine($"[ProductSeeder] Successfully inserted {newProducts.Count} new products.");
            }
            else
            {
                Console.WriteLine("[ProductSeeder] No new products to insert.");
            }

            // Populate mapping dictionary for newly inserted items
            foreach (var mapping in pendingMappings)
            {
                result.IdMap[mapping.JsonId] = mapping.Entity.Id;
            }

            return result;
        }
    }
}
