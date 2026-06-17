using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Repository.Data.DataSeed
{
    //Try again

    public static class UserInteractionSeeder
    {
        public static async Task<int> SeedAsync(
            TalentreeDbContext context,
            string jsonSeedFolderPath,
            Dictionary<int, int> productIdMap,
            Dictionary<int, int> rawMaterialIdMap)
        {
            var totalInserted = 0;

            // 1. Resolve Users
            var ownerUserIds = await context.BusinessOwnerProfiles
                .Select(p => p.UserId)
                .ToListAsync();

            var allUserIds = await context.Users
                .Select(u => u.Id)
                .ToListAsync();

            var customerUserIds = allUserIds.Except(ownerUserIds).ToList();

            if (customerUserIds.Count == 0 && ownerUserIds.Count == 0)
            {
                throw new InvalidOperationException("No valid users exist in the database. Seeding UserInteractions requires users to be seeded first.");
            }

            // 2. Load Product & Raw Material IDs for fallback mapping
            var dbProductIds = await context.Products.Select(p => p.Id).ToListAsync();
            var dbRawMaterialIds = await context.Set<RawMaterial>().Select(rm => rm.Id).ToListAsync();

            // 3. Load Existing Interaction Keys for Idempotency
            Console.WriteLine("[UserInteractionSeeder] Loading existing interactions for duplicate checking...");
            var existingKeys = await context.Set<UserInteraction>()
                .Select(ui => new 
                { 
                    ui.UserId, 
                    ui.ItemId, 
                    ui.ItemType, 
                    ui.ActionType, 
                    ui.InteractionTimestamp 
                })
                .ToListAsync();

            var existingKeysHash = new HashSet<(string UserId, int ItemId, UserInteractionItemType ItemType, UserInteractionActionType ActionType, DateTime Timestamp)>(
                existingKeys.Select(k => (k.UserId, k.ItemId, k.ItemType, k.ActionType, k.InteractionTimestamp))
            );

            var random = new Random();
            var newInteractions = new List<UserInteraction>();
            var localKeysHash = new HashSet<(string UserId, int ItemId, UserInteractionItemType ItemType, UserInteractionActionType ActionType, DateTime Timestamp)>();

            // Files to process
            var fileNames = new[] 
            { 
                "customer_interactions_db_seeding.json", 
                "owner_interactions_db_seeding.json" 
            };

            foreach (var fileName in fileNames)
            {
                var filePath = Path.Combine(jsonSeedFolderPath, fileName);
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"[UserInteractionSeeder] File not found: {filePath} — skipping.");
                    continue;
                }

                Console.WriteLine($"[UserInteractionSeeder] Reading data from {filePath}...");

                var jsonText = await File.ReadAllTextAsync(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var dtos = JsonSerializer.Deserialize<List<UserInteractionSeedDto>>(jsonText, options);
                if (dtos == null || dtos.Count == 0)
                {
                    Console.WriteLine($"[UserInteractionSeeder] No interactions found in {fileName}.");
                    continue;
                }

                var isCustomerFile = fileName.StartsWith("customer_");

                if (isCustomerFile && customerUserIds.Count == 0)
                {
                    throw new InvalidOperationException("No customer users exist in the database to assign to customer interactions.");
                }
                if (!isCustomerFile && ownerUserIds.Count == 0)
                {
                    throw new InvalidOperationException("No business owner users exist in the database to assign to owner interactions.");
                }

                foreach (var dto in dtos)
                {
                    // Assign random valid UserId of the correct partition
                    var assignedUserId = isCustomerFile 
                        ? customerUserIds[random.Next(customerUserIds.Count)]
                        : ownerUserIds[random.Next(ownerUserIds.Count)];

                    // Resolve ItemId using the resolved map or fallback randomly
                    var resolvedItemId = 0;
                    var itemType = (UserInteractionItemType)dto.ItemType;

                    if (itemType == UserInteractionItemType.Product)
                    {
                        if (productIdMap.TryGetValue(dto.ItemId, out var dbProductId))
                        {
                            resolvedItemId = dbProductId;
                        }
                        else
                        {
                            if (dbProductIds.Count == 0)
                            {
                                throw new InvalidOperationException("No products exist in database to map product interactions.");
                            }
                            resolvedItemId = dbProductIds[random.Next(dbProductIds.Count)];
                        }
                    }
                    else if (itemType == UserInteractionItemType.RawMaterial)
                    {
                        if (rawMaterialIdMap.TryGetValue(dto.ItemId, out var dbRawMaterialId))
                        {
                            resolvedItemId = dbRawMaterialId;
                        }
                        else
                        {
                            if (dbRawMaterialIds.Count == 0)
                            {
                                throw new InvalidOperationException("No raw materials exist in database to map raw material interactions.");
                            }
                            resolvedItemId = dbRawMaterialIds[random.Next(dbRawMaterialIds.Count)];
                        }
                    }

                    // Parse Timestamps
                    var timestamp = DateTime.Parse(dto.InteractionTimestamp);
                    var createdAt = string.IsNullOrWhiteSpace(dto.CreatedAt) 
                        ? DateTime.UtcNow 
                        : DateTime.Parse(dto.CreatedAt);

                    var actionType = (UserInteractionActionType)dto.ActionType;
                    var key = (assignedUserId, resolvedItemId, itemType, actionType, timestamp);

                    // Idempotency / Duplicate Check
                    if (existingKeysHash.Contains(key) || localKeysHash.Contains(key))
                    {
                        continue;
                    }

                    localKeysHash.Add(key);

                    newInteractions.Add(new UserInteraction
                    {
                        UserId = assignedUserId,
                        UserType = (UserInteractionType)dto.UserType,
                        ItemId = resolvedItemId,
                        ItemType = itemType,
                        ActionType = actionType,
                        Category = dto.Category,
                        Quantity = dto.Quantity,
                        Price = dto.Price,
                        InteractionTimestamp = timestamp,
                        CreatedAt = createdAt
                    });
                }
            }

            if (newInteractions.Count > 0)
            {
                Console.WriteLine($"[UserInteractionSeeder] Preparing bulk insert of {newInteractions.Count} records using SqlBulkCopy...");

                var dataTable = new DataTable();
                dataTable.Columns.Add("UserId", typeof(string));
                dataTable.Columns.Add("UserType", typeof(int));
                dataTable.Columns.Add("ItemId", typeof(int));
                dataTable.Columns.Add("ItemType", typeof(int));
                dataTable.Columns.Add("ActionType", typeof(int));
                dataTable.Columns.Add("Category", typeof(string));
                dataTable.Columns.Add("Quantity", typeof(int));
                dataTable.Columns.Add("Price", typeof(decimal));
                dataTable.Columns.Add("InteractionTimestamp", typeof(DateTime));
                dataTable.Columns.Add("CreatedAt", typeof(DateTime));

                foreach (var item in newInteractions)
                {
                    dataTable.Rows.Add(
                        item.UserId,
                        (int)item.UserType,
                        item.ItemId,
                        (int)item.ItemType,
                        (int)item.ActionType,
                        item.Category,
                        item.Quantity,
                        item.Price,
                        item.InteractionTimestamp,
                        item.CreatedAt
                    );
                }

                var connection = (SqlConnection)context.Database.GetDbConnection();
                await context.Database.OpenConnectionAsync();

                try
                {
                    var transaction = context.Database.CurrentTransaction?.GetDbTransaction() as SqlTransaction;
                    using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                    {
                        bulkCopy.DestinationTableName = "dbo.UserInteractions";
                        bulkCopy.BatchSize = 10000;
                        bulkCopy.BulkCopyTimeout = 300; // 5 minutes

                        // Explicit column mappings to prevent order issues
                        bulkCopy.ColumnMappings.Add("UserId", "UserId");
                        bulkCopy.ColumnMappings.Add("UserType", "UserType");
                        bulkCopy.ColumnMappings.Add("ItemId", "ItemId");
                        bulkCopy.ColumnMappings.Add("ItemType", "ItemType");
                        bulkCopy.ColumnMappings.Add("ActionType", "ActionType");
                        bulkCopy.ColumnMappings.Add("Category", "Category");
                        bulkCopy.ColumnMappings.Add("Quantity", "Quantity");
                        bulkCopy.ColumnMappings.Add("Price", "Price");
                        bulkCopy.ColumnMappings.Add("InteractionTimestamp", "InteractionTimestamp");
                        bulkCopy.ColumnMappings.Add("CreatedAt", "CreatedAt");

                        await bulkCopy.WriteToServerAsync(dataTable);
                    }

                    totalInserted = newInteractions.Count;
                    Console.WriteLine($"[UserInteractionSeeder] Successfully bulk-inserted {totalInserted} interactions.");
                }
                finally
                {
                    await context.Database.CloseConnectionAsync();
                }
            }
            else
            {
                Console.WriteLine("[UserInteractionSeeder] No new interactions to seed.");
            }

            return totalInserted;
        }
    }
}
