using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.DataSeed
{
    public static class RawMaterialSeeder
    {
        public static async Task<SeederResult<int>> SeedAsync(TalentreeDbContext context, string jsonSeedFolderPath)
        {
            var result = new SeederResult<int>();

            var fileName = "RawMaterials.json";
            var filePath = Path.Combine(jsonSeedFolderPath, fileName);
            if (!File.Exists(filePath))
            {
                filePath = Path.Combine(jsonSeedFolderPath, "raw_materials_db_seeding.json");
            }

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"[RawMaterialSeeder] Seed file not found (tried RawMaterials.json and raw_materials_db_seeding.json) — skipping.");
                return result;
            }

            Console.WriteLine($"[RawMaterialSeeder] Reading data from {filePath}...");

            var jsonText = await File.ReadAllTextAsync(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var dtos = JsonSerializer.Deserialize<List<RawMaterialSeedDto>>(jsonText, options);
            if (dtos == null || dtos.Count == 0)
            {
                Console.WriteLine("[RawMaterialSeeder] No raw materials found in JSON file.");
                return result;
            }

            // 1. Validate foreign keys fail-fast
            var validSupplierIds = await context.Set<Supplier>().Select(s => s.Id).ToListAsync();
            var validSupplierIdsHash = new HashSet<int>(validSupplierIds);

            // 2. Load existing raw materials to prevent duplicates and populate existing maps
            var existingMaterials = await context.Set<RawMaterial>()
                .Select(m => new { m.Id, m.Name })
                .ToListAsync();

            var dbMaterialMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var m in existingMaterials)
            {
                if (!dbMaterialMap.ContainsKey(m.Name))
                {
                    dbMaterialMap[m.Name] = m.Id;
                }
            }

            var newRawMaterials = new List<RawMaterial>();
            var localMaterialMap = new Dictionary<string, RawMaterial>(StringComparer.OrdinalIgnoreCase);
            var pendingMappings = new List<(int JsonId, RawMaterial Entity)>();

            foreach (var dto in dtos)
            {
                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    continue;
                }

                // Foreign Key validation
                if (!validSupplierIdsHash.Contains(dto.SupplierId))
                {
                    throw new InvalidOperationException(
                        $"Foreign key validation failed: SupplierId '{dto.SupplierId}' does not exist in the database for raw material '{dto.Name}'.");
                }

                // Uniqueness check (Case-insensitive)
                if (dbMaterialMap.TryGetValue(dto.Name, out var existingDbId))
                {
                    if (dto.Id.HasValue)
                    {
                        result.IdMap[dto.Id.Value] = existingDbId;
                    }
                    continue;
                }

                // Handle duplicates within the JSON file
                if (localMaterialMap.TryGetValue(dto.Name, out var locallyMappedMaterial))
                {
                    if (dto.Id.HasValue)
                    {
                        pendingMappings.Add((dto.Id.Value, locallyMappedMaterial));
                    }
                    continue;
                }

                // Map DTO to Entity and populate default values when missing
                var rawMaterial = new RawMaterial
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    Price = dto.Price,
                    Unit = dto.Unit,
                    MinimumOrderQuantity = dto.MinimumOrderQuantity,
                    StockQuantity = dto.StockQuantity,
                    IsAvailable = dto.IsAvailable ?? true,
                    Category = dto.Category,
                    PictureUrl = dto.PictureUrl,
                    SupplierId = dto.SupplierId,

                    OrderFrequency = dto.OrderFrequency ?? 0,
                    PriceTrend = string.IsNullOrWhiteSpace(dto.PriceTrend) ? "Stable" : dto.PriceTrend,

                    IsDeleted = dto.IsDeleted ?? false,
                    DeletedAt = dto.DeletedAt,
                    DeletedBy = dto.DeletedBy,

                    CreatedAt = dto.CreatedAt ?? DateTime.UtcNow,
                    UpdatedAt = dto.UpdatedAt,
                    CreatedBy = dto.CreatedBy,
                    UpdatedBy = dto.UpdatedBy
                };

                newRawMaterials.Add(rawMaterial);
                localMaterialMap[dto.Name] = rawMaterial;

                if (dto.Id.HasValue)
                {
                    pendingMappings.Add((dto.Id.Value, rawMaterial));
                }
            }

            if (newRawMaterials.Count > 0)
            {
                await context.Set<RawMaterial>().AddRangeAsync(newRawMaterials);
                await context.SaveChangesAsync();
                result.InsertedCount = newRawMaterials.Count;
                Console.WriteLine($"[RawMaterialSeeder] Successfully inserted {newRawMaterials.Count} new raw materials.");
            }
            else
            {
                Console.WriteLine("[RawMaterialSeeder] No new raw materials to insert.");
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
