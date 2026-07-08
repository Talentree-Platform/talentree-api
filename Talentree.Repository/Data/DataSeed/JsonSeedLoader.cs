// ============================================================
// Talentree.Repository/Data/DataSeed/JsonSeedLoader.cs
// ============================================================
// Reads individual JSON files from the jsonSeed/ folder and
// inserts or updates data into the correct EF DbSets.
//
// Execution order (dependency-safe):
//   0. UsersData               INSERT  (AppUsers + roles — must run first, all other steps FK on users)
//   1. Transactions            INSERT  (no Id — auto-generated)
//   2. LoginHistories          INSERT  (no Id — auto-generated)
//   3. ProductReviews          INSERT  (no Id — auto-generated)
//   4. SupportTickets          INSERT  (no Id — auto-generated, IDs captured for step 5)
//   5. TicketMessages          INSERT  (TicketId resolved via <<SupportTickets[N].Id>> placeholders)
//   6. OnboardingProgress      INSERT  (no Id — auto-generated)
//   7. PayoutRequests          INSERT  (no Id — auto-generated)
//   8. BoProductionRequests    INSERT  (no Id — auto-generated)
//   9. Products_stats_update   UPDATE  (patches ViewCount/AvgRating/etc on existing rows)
//
// All boolean fields in the JSON are stored as 0/1 integers.
// All sections are idempotent: INSERT sections check Any() first.
// ============================================================
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Talentree.Core.Entities;
using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;

namespace Talentree.Repository.Data.DataSeed
{
    public static class JsonSeedLoader
    {
        public static async Task SeedAsync(
            TalentreeDbContext context,
            string jsonSeedFolderPath,
            bool seedInteractions = false,
            UserManager<AppUser>? userManager = null)
        {
            if (!Directory.Exists(jsonSeedFolderPath))
            {
                Console.WriteLine($"[JsonSeedLoader] Folder not found: {jsonSeedFolderPath} — skipping.");
                return;
            }

            Console.WriteLine($"[JsonSeedLoader] Loading from: {jsonSeedFolderPath}");

            // ── 0. Users (must run before every other section — all FK-validate against AppUsers)
            await SeedUsersDataAsync(context, Path.Combine(jsonSeedFolderPath, "UsersData.json"), userManager);

            // ── 0a. Products
            var productSeedResult = await ProductSeeder.SeedAsync(context, jsonSeedFolderPath);

            // ── 0b. Raw Materials
            var rawMaterialSeedResult = await RawMaterialSeeder.SeedAsync(context, jsonSeedFolderPath);

            // ── 0c. User Interactions (Conditional on flag)
            if (seedInteractions)
            {
                await UserInteractionSeeder.SeedAsync(context, jsonSeedFolderPath, productSeedResult.IdMap, rawMaterialSeedResult.IdMap);
            }
            else
            {
                Console.WriteLine("[JsonSeedLoader] Skipping heavy UserInteractions seed (run with --seed-interactions flag to enable).");
            }

            // ── 1. Transactions ──────────────────────────────────────────
            await SeedTransactionsAsync(context, Path.Combine(jsonSeedFolderPath, "Transactions.json"));

            // ── 2. LoginHistories ────────────────────────────────────────
            await SeedLoginHistoriesAsync(context, Path.Combine(jsonSeedFolderPath, "LoginHistories.json"));

            // ── 3. ProductReviews ────────────────────────────────────────
            await SeedProductReviewsAsync(context, Path.Combine(jsonSeedFolderPath, "ProductReviews.json"), productSeedResult.IdMap);

            // ── 4. SupportTickets (must run before TicketMessages) ───────
            var insertedTicketIds = await SeedSupportTicketsAsync(context, Path.Combine(jsonSeedFolderPath, "SupportTickets.json"));

            // ── 5. TicketMessages (resolves <<SupportTickets[N].Id>>) ────
            await SeedTicketMessagesAsync(context, Path.Combine(jsonSeedFolderPath, "TicketMessages.json"), insertedTicketIds);

            // ── 6. OnboardingProgress ────────────────────────────────────
            await SeedOnboardingProgressAsync(context, Path.Combine(jsonSeedFolderPath, "OnboardingProgress.json"));

            // ── 7. PayoutRequests ────────────────────────────────────────
            await SeedPayoutRequestsAsync(context, Path.Combine(jsonSeedFolderPath, "PayoutRequests.json"));

            // ── 8. BoProductionRequests ──────────────────────────────────
            await SeedBoProductionRequestsAsync(context, Path.Combine(jsonSeedFolderPath, "BoProductionRequests.json"));

            // ── 9. Product stats UPDATE ──────────────────────────────────
            await UpdateProductStatsAsync(context, Path.Combine(jsonSeedFolderPath, "Products_stats_update.json"), productSeedResult.IdMap);

            Console.WriteLine("[JsonSeedLoader] ✅ All sections processed.");
        }

        // ──────────────────────────────────────────────────────────────
        // 1. Transactions
        // ──────────────────────────────────────────────────────────────
        private static async Task SeedTransactionsAsync(TalentreeDbContext ctx, string filePath)
        {
            if (!File.Exists(filePath)) { Console.WriteLine($"[JsonSeedLoader] {Path.GetFileName(filePath)} not found — skipping."); return; }

            var existingTransactions = await ctx.Set<Transaction>()
                .Select(t => new { t.BusinessOwnerId, t.Amount, t.CreatedAt })
                .ToListAsync();
            var existingKeysSet = existingTransactions
                .Select(t => (t.BusinessOwnerId, t.Amount, t.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")))
                .ToHashSet();

            using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(filePath));
            var validUserIds = ctx.Set<AppUser>().Select(u => u.Id).ToHashSet();
            var list = new List<Transaction>();

            foreach (var el in doc.RootElement.EnumerateArray())
            {
                var boId = el.GetProperty("BusinessOwnerId").GetString()!;
                if (!validUserIds.Contains(boId)) continue;

                var amount = el.GetProperty("Amount").GetDecimal();
                var createdAt = el.GetProperty("CreatedAt").GetDateTime();
                var createdAtStr = createdAt.ToString("yyyy-MM-dd HH:mm:ss");

                if (existingKeysSet.Contains((boId, amount, createdAtStr)))
                    continue;

                list.Add(new Transaction
                {
                    BusinessOwnerId       = boId,
                    Type                  = el.TryGetProperty("Type", out var tProp) ? ParseEnum<TransactionType>(tProp.GetString() ?? "Sale") : TransactionType.Sale,
                    Description           = el.TryGetProperty("Description", out var descProp) ? descProp.GetString() ?? string.Empty : string.Empty,
                    Amount                = amount,
                    BalanceAfter          = el.TryGetProperty("BalanceAfter", out var balProp) && balProp.ValueKind != JsonValueKind.Null ? balProp.GetDecimal() : 0m,
                    ReferenceId           = GetNullableInt(el, "ReferenceId"),
                    ReferenceType         = GetNullableString(el, "ReferenceType"),
                    StripePaymentIntentId = GetNullableString(el, "StripePaymentIntentId"),
                    AnomalyFlag           = el.TryGetProperty("AnomalyFlag", out var af) && af.ValueKind != JsonValueKind.Null && (af.ValueKind == JsonValueKind.True || (af.ValueKind == JsonValueKind.Number && af.GetInt32() != 0)),
                    AnomalyScore          = GetNullableFloat(el, "AnomalyScore"),
                    CreatedAt             = createdAt,
                    UpdatedAt             = el.TryGetProperty("UpdatedAt", out var uaProp) && uaProp.ValueKind != JsonValueKind.Null ? uaProp.GetDateTime() : createdAt,
                    CreatedBy             = el.TryGetProperty("CreatedBy", out var cbProp) && cbProp.ValueKind != JsonValueKind.Null ? cbProp.GetString() ?? boId : boId,
                    UpdatedBy             = el.TryGetProperty("UpdatedBy", out var ubProp) && ubProp.ValueKind != JsonValueKind.Null ? ubProp.GetString() ?? boId : boId,
                });
            }

            if (list.Count > 0)
            {
                ctx.Set<Transaction>().AddRange(list);
                await ctx.SaveChangesAsync();
                Console.WriteLine($"[JsonSeedLoader] Transactions → {list.Count} rows inserted.");
            }
            else
            {
                Console.WriteLine("[JsonSeedLoader] Transactions → No new transactions to seed.");
            }
        }

        // ──────────────────────────────────────────────────────────────
        // 2. LoginHistories
        // ──────────────────────────────────────────────────────────────
        private static async Task SeedLoginHistoriesAsync(TalentreeDbContext ctx, string filePath)
        {
            if (!File.Exists(filePath)) { Console.WriteLine($"[JsonSeedLoader] {Path.GetFileName(filePath)} not found — skipping."); return; }

            var existingLogins = await ctx.Set<LoginHistory>()
                .Select(l => new { l.UserId, l.LoginAt })
                .ToListAsync();
            var existingKeysSet = existingLogins
                .Select(l => (l.UserId, l.LoginAt.ToString("yyyy-MM-dd HH:mm:ss")))
                .ToHashSet();

            using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(filePath));
            var validUserIds = ctx.Set<AppUser>().Select(u => u.Id).ToHashSet();
            var list = new List<LoginHistory>();

            foreach (var el in doc.RootElement.EnumerateArray())
            {
                var userId = el.GetProperty("UserId").GetString()!;
                if (!validUserIds.Contains(userId)) continue;

                var loginAt = el.GetProperty("LoginAt").GetDateTime();
                var loginAtStr = loginAt.ToString("yyyy-MM-dd HH:mm:ss");

                if (existingKeysSet.Contains((userId, loginAtStr)))
                    continue;

                list.Add(new LoginHistory
                {
                    UserId       = userId,
                    IpAddress    = el.GetProperty("IpAddress").GetString() ?? string.Empty,
                    DeviceInfo   = GetNullableString(el, "DeviceInfo"),
                    Location     = GetNullableString(el, "Location"),
                    LoginAt      = loginAt,
                    IsSuccessful = el.TryGetProperty("IsSuccessful", out var isSucc) && isSucc.ValueKind != JsonValueKind.Null && (isSucc.ValueKind == JsonValueKind.True || (isSucc.ValueKind == JsonValueKind.Number && isSucc.GetInt32() != 0)),
                });
            }

            if (list.Count > 0)
            {
                ctx.Set<LoginHistory>().AddRange(list);
                await ctx.SaveChangesAsync();
                Console.WriteLine($"[JsonSeedLoader] LoginHistories → {list.Count} rows inserted.");
            }
            else
            {
                Console.WriteLine("[JsonSeedLoader] LoginHistories → No new logins to seed.");
            }
        }

        // ──────────────────────────────────────────────────────────────
        // 3. ProductReviews
        // ──────────────────────────────────────────────────────────────
        private static async Task SeedProductReviewsAsync(TalentreeDbContext ctx, string filePath, Dictionary<int, int> productIdMap)
        {
            if (!File.Exists(filePath)) { Console.WriteLine($"[JsonSeedLoader] {Path.GetFileName(filePath)} not found — skipping."); return; }

            var existingReviews = await ctx.Set<ProductReview>()
                .Select(r => new { r.CustomerUserId, r.ProductId, r.CreatedAt })
                .ToListAsync();
            var existingKeysSet = existingReviews
                .Select(r => (r.CustomerUserId, r.ProductId, r.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")))
                .ToHashSet();

            using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(filePath));
            var validProductIds = ctx.Set<Product>().Select(p => p.Id).ToHashSet();
            var validUserIds    = ctx.Set<AppUser>().Select(u => u.Id).ToHashSet();
            var list = new List<ProductReview>();

            foreach (var el in doc.RootElement.EnumerateArray())
            {
                var jsonProductId = el.TryGetProperty("ProductId", out var pIdProp) ? pIdProp.GetInt32() : 0;
                if (jsonProductId == 0) continue;

                if (!productIdMap.TryGetValue(jsonProductId, out var productId))
                {
                    productId = jsonProductId;
                }

                if (!validProductIds.Contains(productId)) continue;

                var customerUserId = el.TryGetProperty("CustomerUserId", out var cuProp) ? cuProp.GetString() : null;
                if (string.IsNullOrEmpty(customerUserId) || !validUserIds.Contains(customerUserId)) continue;

                var createdAt = el.TryGetProperty("CreatedAt", out var caProp) && caProp.ValueKind != JsonValueKind.Null ? caProp.GetDateTime() : DateTime.UtcNow;
                var createdAtStr = createdAt.ToString("yyyy-MM-dd HH:mm:ss");

                if (existingKeysSet.Contains((customerUserId, productId, createdAtStr)))
                    continue;

                list.Add(new ProductReview
                {
                    ProductId      = productId,
                    CustomerUserId = customerUserId,
                    CustomerName   = el.TryGetProperty("CustomerName", out var cn) ? cn.GetString() ?? string.Empty : string.Empty,
                    Rating         = el.TryGetProperty("Rating", out var rat) && rat.ValueKind != JsonValueKind.Null ? (byte)rat.GetInt32() : (byte)5,
                    ReviewText     = GetNullableString(el, "ReviewText"),
                    IsAnonymous    = GetIntBool(el, "IsAnonymous"),
                    OwnerResponse  = GetNullableString(el, "OwnerResponse"),
                    ResponseAt     = GetNullableDateTime(el, "ResponseAt"),
                    SentimentScore = GetNullableFloat(el, "SentimentScore"),
                    SentimentLabel = GetNullableString(el, "SentimentLabel"),
                    FlaggedToxic   = GetIntBool(el, "FlaggedToxic"),
                    CreatedAt      = createdAt,
                    UpdatedAt      = el.TryGetProperty("UpdatedAt", out var uaProp) && uaProp.ValueKind != JsonValueKind.Null ? uaProp.GetDateTime() : createdAt,
                    CreatedBy      = el.TryGetProperty("CreatedBy", out var cbProp) && cbProp.ValueKind != JsonValueKind.Null ? cbProp.GetString() ?? customerUserId : customerUserId,
                    UpdatedBy      = el.TryGetProperty("UpdatedBy", out var ubProp) && ubProp.ValueKind != JsonValueKind.Null ? ubProp.GetString() ?? customerUserId : customerUserId,
                });
            }

            if (list.Count > 0)
            {
                ctx.Set<ProductReview>().AddRange(list);
                await ctx.SaveChangesAsync();
                Console.WriteLine($"[JsonSeedLoader] ProductReviews → {list.Count} rows inserted.");
            }
            else
            {
                Console.WriteLine("[JsonSeedLoader] ProductReviews → No new reviews to seed.");
            }
        }

        // ──────────────────────────────────────────────────────────────
        // 4. SupportTickets
        // Returns the ordered list of auto-generated IDs so that
        // TicketMessages can resolve <<SupportTickets[N].Id>> placeholders.
        // ──────────────────────────────────────────────────────────────
        private static async Task<List<int>> SeedSupportTicketsAsync(TalentreeDbContext ctx, string filePath)
        {
            if (!File.Exists(filePath)) { Console.WriteLine($"[JsonSeedLoader] {Path.GetFileName(filePath)} not found — skipping."); return new List<int>(); }

            var dbTickets = await ctx.Set<SupportTicket>()
                .ToDictionaryAsync(t => t.TicketNumber, t => t.Id);

            var insertedTicketIds = new List<int>();
            using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(filePath));
            var validUserIds = ctx.Set<AppUser>().Select(u => u.Id).ToHashSet();

            var ticketsToInsert = new List<(SupportTicket Ticket, int JsonIndex)>();
            var index = 0;

            foreach (var el in doc.RootElement.EnumerateArray())
            {
                var ticketNumber = el.GetProperty("TicketNumber").GetString() ?? string.Empty;
                var boUserId = el.GetProperty("BusinessOwnerUserId").GetString()!;
                if (!validUserIds.Contains(boUserId))
                {
                    insertedTicketIds.Add(0);
                    index++;
                    continue;
                }

                if (dbTickets.TryGetValue(ticketNumber, out var existingId))
                {
                    insertedTicketIds.Add(existingId);
                }
                else
                {
                    var ticket = new SupportTicket
                    {
                        BusinessOwnerUserId = boUserId,
                        Category            = el.TryGetProperty("Category", out var catProp) ? (TicketCategory)catProp.GetInt32() : TicketCategory.Other,
                        Subject             = el.TryGetProperty("Subject", out var subjProp) ? subjProp.GetString() ?? string.Empty : string.Empty,
                        Description         = el.TryGetProperty("Description", out var descProp) ? descProp.GetString() ?? string.Empty : string.Empty,
                        Status              = el.TryGetProperty("Status", out var statProp) ? (TicketStatus)statProp.GetInt32() : TicketStatus.Open,
                        Priority            = el.TryGetProperty("Priority", out var prioProp) ? (TicketPriority)prioProp.GetInt32() : TicketPriority.Normal,
                        TicketNumber        = ticketNumber,
                        IsDeleted           = GetIntBool(el, "IsDeleted"),
                        CreatedAt           = el.TryGetProperty("CreatedAt", out var crAt) ? crAt.GetDateTime() : DateTime.UtcNow,
                        UpdatedAt           = el.TryGetProperty("UpdatedAt", out var upAt) ? upAt.GetDateTime() : DateTime.UtcNow,
                        CreatedBy           = el.TryGetProperty("CreatedBy", out var cbProp) ? cbProp.GetString() ?? boUserId : boUserId,
                        UpdatedBy           = el.TryGetProperty("UpdatedBy", out var ubProp) ? ubProp.GetString() ?? boUserId : boUserId,
                    };

                    ticketsToInsert.Add((ticket, index));
                    insertedTicketIds.Add(0);
                }
                index++;
            }

            if (ticketsToInsert.Count > 0)
            {
                await ctx.Set<SupportTicket>().AddRangeAsync(ticketsToInsert.Select(t => t.Ticket));
                await ctx.SaveChangesAsync();

                foreach (var (ticket, jsonIdx) in ticketsToInsert)
                {
                    insertedTicketIds[jsonIdx] = ticket.Id;
                }
                Console.WriteLine($"[JsonSeedLoader] SupportTickets → {ticketsToInsert.Count} rows inserted.");
            }
            else
            {
                Console.WriteLine("[JsonSeedLoader] SupportTickets → No new support tickets to seed.");
            }

            return insertedTicketIds;
        }

        // ──────────────────────────────────────────────────────────────
        // 5. TicketMessages
        // TicketId field contains "<<SupportTickets[N].Id>>" (1-based index).
        // We resolve this using the ordered list returned from step 4.
        // ──────────────────────────────────────────────────────────────
        private static async Task SeedTicketMessagesAsync(TalentreeDbContext ctx, string filePath, List<int> ticketIds)
        {
            if (!File.Exists(filePath)) { Console.WriteLine($"[JsonSeedLoader] {Path.GetFileName(filePath)} not found — skipping."); return; }
            if (ticketIds.Count == 0) { Console.WriteLine("[JsonSeedLoader] No ticket IDs available — skipping TicketMessages."); return; }

            var existingMessages = await ctx.Set<TicketMessage>()
                .Select(m => new { m.TicketId, m.SenderId, m.CreatedAt })
                .ToListAsync();
            var existingKeysSet = existingMessages
                .Select(m => (m.TicketId, m.SenderId, m.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")))
                .ToHashSet();

            using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(filePath));
            var validUserIds = ctx.Set<AppUser>().Select(u => u.Id).ToHashSet();
            var list = new List<TicketMessage>();
            var skipped = 0;

            foreach (var el in doc.RootElement.EnumerateArray())
            {
                var ticketIdRaw = el.GetProperty("TicketId").GetString() ?? string.Empty;

                // Resolve <<SupportTickets[N].Id>> placeholder (1-based)
                int resolvedTicketId;
                if (ticketIdRaw.StartsWith("<<SupportTickets[") && ticketIdRaw.EndsWith("].Id>>"))
                {
                    var indexStr = ticketIdRaw["<<SupportTickets[".Length..^"].Id>>".Length];
                    if (!int.TryParse(indexStr, out var idx) || idx < 1 || idx > ticketIds.Count)
                    { skipped++; continue; }
                    resolvedTicketId = ticketIds[idx - 1];
                }
                else if (int.TryParse(ticketIdRaw, out var directId))
                {
                    resolvedTicketId = directId;
                }
                else { skipped++; continue; }

                var senderId = el.GetProperty("SenderId").GetString()!;
                if (!validUserIds.Contains(senderId)) { skipped++; continue; }

                var createdAt = el.GetProperty("CreatedAt").GetDateTime();
                var createdAtStr = createdAt.ToString("yyyy-MM-dd HH:mm:ss");

                if (existingKeysSet.Contains((resolvedTicketId, senderId, createdAtStr)))
                    continue;

                list.Add(new TicketMessage
                {
                    TicketId       = resolvedTicketId,
                    SenderId       = senderId,
                    Content        = el.TryGetProperty("Content", out var contProp) ? contProp.GetString() ?? string.Empty : string.Empty,
                    IsAdminMessage = GetIntBool(el, "IsAdminMessage"),
                    EmailSent      = GetIntBool(el, "EmailSent"),
                    CreatedAt      = createdAt,
                    UpdatedAt      = el.TryGetProperty("UpdatedAt", out var upAtProp) ? upAtProp.GetDateTime() : createdAt,
                });
            }

            if (list.Count > 0)
            {
                ctx.Set<TicketMessage>().AddRange(list);
                await ctx.SaveChangesAsync();
                Console.WriteLine($"[JsonSeedLoader] TicketMessages → {list.Count} rows inserted ({skipped} skipped).");
            }
            else
            {
                Console.WriteLine("[JsonSeedLoader] TicketMessages → No new ticket messages to seed.");
            }
        }

        // ──────────────────────────────────────────────────────────────
        // 6. OnboardingProgress
        // ──────────────────────────────────────────────────────────────
        private static async Task SeedOnboardingProgressAsync(TalentreeDbContext ctx, string filePath)
        {
            if (!File.Exists(filePath)) { Console.WriteLine($"[JsonSeedLoader] {Path.GetFileName(filePath)} not found — skipping."); return; }

            var existingKeysSet = (await ctx.Set<OnboardingProgress>()
                .Select(o => o.BusinessOwnerId)
                .ToListAsync())
                .ToHashSet();

            using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(filePath));
            var validUserIds = ctx.Set<AppUser>().Select(u => u.Id).ToHashSet();
            var list = new List<OnboardingProgress>();

            foreach (var el in doc.RootElement.EnumerateArray())
            {
                var boId = el.GetProperty("BusinessOwnerId").GetString()!;
                if (!validUserIds.Contains(boId)) continue;

                if (existingKeysSet.Contains(boId))
                    continue;

                list.Add(new OnboardingProgress
                {
                    BusinessOwnerId       = boId,
                    TourCompleted         = GetIntBool(el, "TourCompleted"),
                    ChecklistProductAdded = GetIntBool(el, "ChecklistProductAdded"),
                    ChecklistPaymentSet   = GetIntBool(el, "ChecklistPaymentSet"),
                    ChecklistProfileDone  = GetIntBool(el, "ChecklistProfileDone"),
                });
            }

            if (list.Count > 0)
            {
                ctx.Set<OnboardingProgress>().AddRange(list);
                await ctx.SaveChangesAsync();
                Console.WriteLine($"[JsonSeedLoader] OnboardingProgress → {list.Count} rows inserted.");
            }
            else
            {
                Console.WriteLine("[JsonSeedLoader] OnboardingProgress → No new onboarding progress to seed.");
            }
        }

        // ──────────────────────────────────────────────────────────────
        // 7. PayoutRequests
        // ──────────────────────────────────────────────────────────────
        private static async Task SeedPayoutRequestsAsync(TalentreeDbContext ctx, string filePath)
        {
            if (!File.Exists(filePath)) { Console.WriteLine($"[JsonSeedLoader] {Path.GetFileName(filePath)} not found — skipping."); return; }

            var existingPayouts = await ctx.Set<PayoutRequest>()
                .Select(p => new { p.BusinessOwnerId, p.Status, p.CreatedAt })
                .ToListAsync();
            var existingKeysSet = existingPayouts
                .Select(k => (k.BusinessOwnerId, k.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")))
                .ToHashSet();

            var existingPendingBoIds = existingPayouts
                .Where(p => p.Status == PayoutStatus.Pending)
                .Select(p => p.BusinessOwnerId)
                .ToHashSet();

            using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(filePath));
            var validUserIds = ctx.Set<AppUser>().Select(u => u.Id).ToHashSet();
            var list = new List<PayoutRequest>();

            foreach (var el in doc.RootElement.EnumerateArray())
            {
                var boId = el.GetProperty("BusinessOwnerId").GetString()!;
                if (!validUserIds.Contains(boId)) continue;

                var createdAt = el.GetProperty("CreatedAt").GetDateTime();
                var createdAtStr = createdAt.ToString("yyyy-MM-dd HH:mm:ss");

                if (existingKeysSet.Contains((boId, createdAtStr)))
                    continue;

                var status = ParseEnum<PayoutStatus>(el.GetProperty("Status").GetString() ?? "Pending");

                // Enforce unique index: only one Pending payout request per BO
                if (status == PayoutStatus.Pending && existingPendingBoIds.Contains(boId))
                {
                    Console.WriteLine($"[JsonSeedLoader] Skipping Pending payout request for BusinessOwner {boId} because one already exists in the database.");
                    continue;
                }

                list.Add(new PayoutRequest
                {
                    BusinessOwnerId      = boId,
                    Amount               = el.TryGetProperty("Amount", out var amtProp) ? amtProp.GetDecimal() : 0m,
                    Currency             = el.TryGetProperty("Currency", out var currProp) ? currProp.GetString() ?? "EGP" : "EGP",
                    Status               = status,
                    BankName             = GetNullableString(el, "BankName"),
                    AccountHolderName    = GetNullableString(el, "AccountHolderName"),
                    AccountIdentifierEnc = GetNullableString(el, "AccountIdentifierEnc"),
                    RoutingSwiftCode     = GetNullableString(el, "RoutingSwiftCode"),
                    RejectionReason      = GetNullableString(el, "RejectionReason"),
                    ProcessedAt          = GetNullableDateTime(el, "ProcessedAt"),
                    ProcessedBy          = GetNullableString(el, "ProcessedBy"),
                    CreatedAt            = createdAt,
                    UpdatedAt            = el.TryGetProperty("UpdatedAt", out var upAtProp) ? upAtProp.GetDateTime() : createdAt,
                    CreatedBy            = el.TryGetProperty("CreatedBy", out var cbProp) ? cbProp.GetString() ?? boId : boId,
                    UpdatedBy            = el.TryGetProperty("UpdatedBy", out var ubProp) ? ubProp.GetString() ?? boId : boId,
                });
            }

            if (list.Count > 0)
            {
                ctx.Set<PayoutRequest>().AddRange(list);
                await ctx.SaveChangesAsync();
                Console.WriteLine($"[JsonSeedLoader] PayoutRequests → {list.Count} rows inserted.");
            }
            else
            {
                Console.WriteLine("[JsonSeedLoader] PayoutRequests → No new payout requests to seed.");
            }
        }

        // ──────────────────────────────────────────────────────────────
        // 8. BoProductionRequests
        // ──────────────────────────────────────────────────────────────
        private static async Task SeedBoProductionRequestsAsync(TalentreeDbContext ctx, string filePath)
        {
            if (!File.Exists(filePath)) { Console.WriteLine($"[JsonSeedLoader] {Path.GetFileName(filePath)} not found — skipping."); return; }

            var existingRequests = await ctx.Set<BoProductionRequest>()
                .Select(r => new { r.BusinessOwnerId, r.Title, r.CreatedAt })
                .ToListAsync();
            var existingKeysSet = existingRequests
                .Select(k => (k.BusinessOwnerId, k.Title, k.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")))
                .ToHashSet();

            using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(filePath));
            var validUserIds = ctx.Set<AppUser>().Select(u => u.Id).ToHashSet();
            var list = new List<BoProductionRequest>();

            foreach (var el in doc.RootElement.EnumerateArray())
            {
                var boId = el.GetProperty("BusinessOwnerId").GetString()!;
                if (!validUserIds.Contains(boId)) continue;

                var title = el.GetProperty("Title").GetString() ?? string.Empty;
                var createdAt = el.GetProperty("CreatedAt").GetDateTime();
                var createdAtStr = createdAt.ToString("yyyy-MM-dd HH:mm:ss");

                if (existingKeysSet.Contains((boId, title, createdAtStr)))
                    continue;

                list.Add(new BoProductionRequest
                {
                    BusinessOwnerId         = boId,
                    Title                   = title,
                    Notes                   = GetNullableString(el, "Notes"),
                    Status                  = el.TryGetProperty("Status", out var statProp) ? ParseEnum<BoProductionRequestStatus>(statProp.GetString() ?? "Submitted") : BoProductionRequestStatus.Submitted,
                    QuotedPrice             = el.TryGetProperty("QuotedPrice", out var priceProp) ? priceProp.GetDecimal() : 0m,
                    AdminNotes              = GetNullableString(el, "AdminNotes"),
                    EstimatedCompletionDate = GetNullableDateTime(el, "EstimatedCompletionDate"),
                    CompletedAt             = GetNullableDateTime(el, "CompletedAt"),
                    FraudScore              = GetNullableFloat(el, "FraudScore"),
                    FulfillmentTimeHours    = GetNullableInt(el, "FulfillmentTimeHours"),
                    IsFraudFlag             = GetIntBool(el, "IsFraudFlag"),
                    PaymentStatus           = el.TryGetProperty("PaymentStatus", out var psProp) ? ParseEnum<PaymentStatus>(psProp.GetString() ?? "Unpaid") : PaymentStatus.Unpaid,
                    StripePaymentIntentId   = GetNullableString(el, "StripePaymentIntentId"),
                    CreatedAt               = createdAt,
                    UpdatedAt               = el.TryGetProperty("UpdatedAt", out var upAtProp) ? upAtProp.GetDateTime() : createdAt,
                    CreatedBy               = el.TryGetProperty("CreatedBy", out var cbProp) ? cbProp.GetString() ?? boId : boId,
                    UpdatedBy               = el.TryGetProperty("UpdatedBy", out var ubProp) ? ubProp.GetString() ?? boId : boId,
                });
            }

            if (list.Count > 0)
            {
                ctx.Set<BoProductionRequest>().AddRange(list);
                await ctx.SaveChangesAsync();
                Console.WriteLine($"[JsonSeedLoader] BoProductionRequests → {list.Count} rows inserted.");
            }
            else
            {
                Console.WriteLine("[JsonSeedLoader] BoProductionRequests → No new production requests to seed.");
            }
        }

        // ──────────────────────────────────────────────────────────────
        // 9. Products_stats_update — UPDATE only, not INSERT
        // Patches ViewCount, CartAddCount, PurchaseCount, RevenueTotal,
        // AvgRating on existing Product rows identified by Id.
        // ──────────────────────────────────────────────────────────────
        private static async Task UpdateProductStatsAsync(TalentreeDbContext ctx, string filePath, Dictionary<int, int> productIdMap)
        {
            if (!File.Exists(filePath)) { Console.WriteLine($"[JsonSeedLoader] {Path.GetFileName(filePath)} not found — skipping."); return; }

            using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(filePath));
            var updated = 0;

            foreach (var el in doc.RootElement.EnumerateArray())
            {
                var jsonProductId = el.GetProperty("Id").GetInt32();
                if (!productIdMap.TryGetValue(jsonProductId, out var productId))
                {
                    productId = jsonProductId;
                }

                var product = await ctx.Set<Product>().FindAsync(productId);
                if (product == null) continue;

                if (el.TryGetProperty("ViewCount",    out var vc))  product.ViewCount    = vc.GetInt32();
                if (el.TryGetProperty("CartAddCount", out var cc))  product.CartAddCount = cc.GetInt32();
                if (el.TryGetProperty("PurchaseCount",out var pc))  product.PurchaseCount= pc.GetInt32();
                if (el.TryGetProperty("RevenueTotal", out var rt))  product.RevenueTotal = rt.GetDecimal();
                if (el.TryGetProperty("AvgRating",    out var ar))  product.AvgRating    = (float)ar.GetDouble();

                updated++;
            }

            await ctx.SaveChangesAsync();
            Console.WriteLine($"[JsonSeedLoader] Products_stats_update → {updated} rows updated.");
        }

        // ──────────────────────────────────────────────────────────────
        // 0. UsersData
        // Seeds AppUser rows from UsersData.json.
        // Requires UserManager to hash passwords and assign roles correctly.
        // Gracefully degrades (warning only) when userManager is null or
        // the file does not exist / is empty.
        // Each JSON object may carry an optional "Role" string field;
        // defaults to "Customer" when the field is absent.
        // Idempotency sentinel: first user's email (customer001@seed.talentree.test).
        // ──────────────────────────────────────────────────────────────
        private static async Task SeedUsersDataAsync(
            TalentreeDbContext ctx,
            string filePath,
            UserManager<AppUser>? userManager)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("[JsonSeedLoader] UsersData.json not found — skipping.");
                return;
            }

            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length == 0)
            {
                Console.WriteLine("[JsonSeedLoader] UsersData.json is empty — skipping.");
                return;
            }

            if (userManager == null)
            {
                Console.WriteLine("[JsonSeedLoader] WARNING: UserManager not provided — skipping UsersData seed.");
                return;
            }

            // Idempotency check — sentinel is the first seeded user's e-mail.
            const string SeedPassword = "Seed@Talentree2026";
            const string SentinelEmail = "customer001@seed.talentree.test";

            using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(filePath));
            var tablesElement = doc.RootElement.GetProperty("tables");
            var usersElement = tablesElement.GetProperty("AspNetUsers");
            var rolesElement = tablesElement.GetProperty("AspNetUserRoles");
            var profilesElement = tablesElement.GetProperty("BusinessOwnerProfile");

            // ── 1. Read Role Mappings ──────────────────────────────────
            var userIdToRoleName = new Dictionary<string, string>();
            foreach (var el in rolesElement.EnumerateArray())
            {
                var userId = el.GetProperty("UserId").GetString();
                var roleId = el.GetProperty("RoleId").GetString();
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roleId)) continue;

                var roleName = roleId switch
                {
                    "d36bb1ec-0086-4bb2-87f6-74e4317e9f25" => "Customer",
                    "2dc54145-70b1-418c-83aa-bdf412133d88" => "BusinessOwner",
                    _ => "Customer"
                };
                userIdToRoleName[userId] = roleName;
            }

            // ── 2. Seed AspNetUsers ────────────────────────────────────
            var inserted = 0;
            var skipped  = 0;

            foreach (var el in usersElement.EnumerateArray())
            {
                var email = el.GetProperty("Email").GetString();
                if (string.IsNullOrWhiteSpace(email)) { skipped++; continue; }

                var userId = el.GetProperty("Id").GetString();
                if (string.IsNullOrEmpty(userId)) { skipped++; continue; }

                // Skip if this specific user already exists (partial-run safety)
                if (await ctx.Users.AnyAsync(u => u.Email == email))
                {
                    skipped++;
                    continue;
                }

                // ── Build AppUser from JSON ─────────────────────────────
                var user = new AppUser
                {
                    // Identity core fields
                    Id                   = userId,
                    DisplayName          = el.GetProperty("DisplayName").GetString() ?? string.Empty,
                    Email                = email,
                    UserName             = el.GetProperty("UserName").GetString() ?? email,
                    NormalizedEmail      = (el.GetProperty("NormalizedEmail").GetString() ?? email).ToUpperInvariant(),
                    NormalizedUserName   = (el.GetProperty("NormalizedUserName").GetString() ?? email).ToUpperInvariant(),
                    EmailConfirmed       = el.TryGetProperty("EmailConfirmed", out var ec) && ec.GetBoolean(),
                    PhoneNumberConfirmed = el.TryGetProperty("PhoneNumberConfirmed", out var pnc) && pnc.GetBoolean(),
                    TwoFactorEnabled     = el.TryGetProperty("TwoFactorEnabled", out var tfe) && tfe.GetBoolean(),
                    LockoutEnabled       = el.TryGetProperty("LockoutEnabled", out var le) && le.GetBoolean(),
                    AccessFailedCount    = el.TryGetProperty("AccessFailedCount", out var afc) ? afc.GetInt32() : 0,
                    SecurityStamp        = el.TryGetProperty("SecurityStamp", out var ss) && ss.ValueKind != JsonValueKind.Null
                                               ? ss.GetString()! : Guid.NewGuid().ToString(),
                    ConcurrencyStamp     = el.TryGetProperty("ConcurrencyStamp", out var cs) && cs.ValueKind != JsonValueKind.Null
                                               ? cs.GetString()! : Guid.NewGuid().ToString(),

                    // AppUser custom fields
                    IsActive             = !el.TryGetProperty("IsActive", out var ia) || ia.GetBoolean(),
                    LoginCount           = el.TryGetProperty("LoginCount", out var lc) ? lc.GetInt32() : 0,
                    AccountStatus        = el.TryGetProperty("AccountStatus", out var ast)
                                               ? (AccountStatus)ast.GetInt32()
                                               : AccountStatus.Active,
                    IsBlocked            = el.TryGetProperty("IsBlocked", out var ib) && ib.GetBoolean(),
                    LoginAttempts        = el.TryGetProperty("LoginAttempts", out var lat) ? lat.GetInt32() : 0,
                    IsTwoFactorEnabled   = el.TryGetProperty("IsTwoFactorEnabled", out var itfe) && itfe.GetBoolean(),
                    MustChangePassword   = el.TryGetProperty("MustChangePassword", out var mcp) && mcp.GetBoolean(),
                    CreatedAt            = el.TryGetProperty("CreatedAt", out var cat) && cat.ValueKind != JsonValueKind.Null
                                               ? cat.GetDateTime() : DateTime.UtcNow,
                    LastLoginAt          = GetNullableDateTime(el, "LastLoginAt"),
                };

                // ── Determine role ──────────────────────────────────────
                if (!userIdToRoleName.TryGetValue(userId, out var role))
                {
                    role = "Customer";
                }

                // ── Create via UserManager (handles password hashing + stores user) ──
                var result = await userManager.CreateAsync(user, SeedPassword);
                if (!result.Succeeded)
                {
                    Console.WriteLine($"[JsonSeedLoader] WARNING: Could not seed user '{email}': " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                    skipped++;
                    continue;
                }

                await userManager.AddToRoleAsync(user, role);
                inserted++;
            }

            Console.WriteLine($"[JsonSeedLoader] AspNetUsers → {inserted} users inserted, {skipped} skipped.");

            // ── 3. Seed BusinessOwnerProfile ───────────────────────────
            var profileInserted = 0;
            var profileSkipped = 0;

            foreach (var el in profilesElement.EnumerateArray())
            {
                var userId = el.GetProperty("UserId").GetString();
                if (string.IsNullOrEmpty(userId)) { profileSkipped++; continue; }

                // Skip if profile already exists in DB
                if (await ctx.BusinessOwnerProfiles.AnyAsync(p => p.UserId == userId))
                {
                    profileSkipped++;
                    continue;
                }

                // Verify user actually exists in AspNetUsers before inserting profile to avoid FK violation
                if (!await ctx.Users.AnyAsync(u => u.Id == userId))
                {
                    profileSkipped++;
                    continue;
                }

                var profile = new BusinessOwnerProfile
                {
                    UserId                 = userId,
                    BusinessName           = el.GetProperty("BusinessName").GetString() ?? string.Empty,
                    BusinessDescription    = el.GetProperty("BusinessDescription").GetString() ?? string.Empty,
                    BusinessCategory       = el.GetProperty("BusinessCategory").GetString() ?? string.Empty,
                    BusinessAddress        = GetNullableString(el, "BusinessAddress"),
                    TaxId                  = GetNullableString(el, "TaxId"),
                    FacebookLink           = GetNullableString(el, "FacebookLink"),
                    InstagramLink          = GetNullableString(el, "InstagramLink"),
                    WebsiteLink            = GetNullableString(el, "WebsiteLink"),
                    ProfilePhotoUrl        = GetNullableString(el, "ProfilePhotoUrl"),
                    BusinessLogoUrl        = GetNullableString(el, "BusinessLogoUrl"),
                    PhoneNumber            = GetNullableString(el, "PhoneNumber"),
                    ProfileCompletenessPct = el.TryGetProperty("ProfileCompletenessPct", out var pcp) ? (byte)pcp.GetInt32() : (byte)0,
                    Status                 = el.TryGetProperty("Status", out var st) ? (ApprovalStatus)st.GetInt32() : ApprovalStatus.Pending,
                    ApprovedAt             = GetNullableDateTime(el, "ApprovedAt"),
                    ApprovedBy             = GetNullableString(el, "ApprovedBy"),
                    RejectionReason        = GetNullableString(el, "RejectionReason"),
                    AutoApprovalDeadline   = GetNullableDateTime(el, "AutoApprovalDeadline"),
                    IsDeleted              = el.TryGetProperty("IsDeleted", out var isDel) && isDel.GetBoolean(),
                    DeletedAt              = GetNullableDateTime(el, "DeletedAt"),
                    DeletedBy              = GetNullableString(el, "DeletedBy"),
                    TargetAudience         = GetNullableString(el, "TargetAudience"),
                    BrandTone              = GetNullableString(el, "BrandTone"),
                    CreatedAt              = el.TryGetProperty("CreatedAt", out var cat) && cat.ValueKind != JsonValueKind.Null ? cat.GetDateTime() : DateTime.UtcNow,
                    UpdatedAt              = GetNullableDateTime(el, "UpdatedAt"),
                    CreatedBy              = el.TryGetProperty("CreatedBy", out var cb) && cb.ValueKind != JsonValueKind.Null ? cb.GetString() : "SystemSeeder",
                    UpdatedBy              = el.TryGetProperty("UpdatedBy", out var ub) && ub.ValueKind != JsonValueKind.Null ? ub.GetString() : "SystemSeeder"
                };

                ctx.BusinessOwnerProfiles.Add(profile);
                profileInserted++;
            }

            if (profileInserted > 0)
            {
                await ctx.SaveChangesAsync();
            }

            Console.WriteLine($"[JsonSeedLoader] BusinessOwnerProfile → {profileInserted} profiles inserted, {profileSkipped} skipped.");
        }

        // ──────────────────────────────────────────────────────────────
        // Helpers
        // ──────────────────────────────────────────────────────────────
        private static T ParseEnum<T>(string value) where T : struct, Enum
            => Enum.TryParse<T>(value, ignoreCase: true, out var r) ? r : default;

        /// <summary>Reads a JSON int or bool field as bool.</summary>
        private static bool GetIntBool(JsonElement el, string prop)
        {
            if (!el.TryGetProperty(prop, out var v) || v.ValueKind == JsonValueKind.Null)
                return false;
            if (v.ValueKind == JsonValueKind.True) return true;
            if (v.ValueKind == JsonValueKind.False) return false;
            if (v.ValueKind == JsonValueKind.Number) return v.GetInt32() != 0;
            return false;
        }

        private static string? GetNullableString(JsonElement el, string prop)
            => el.TryGetProperty(prop, out var v) && v.ValueKind != JsonValueKind.Null ? v.GetString() : null;

        private static DateTime? GetNullableDateTime(JsonElement el, string prop)
            => el.TryGetProperty(prop, out var v) && v.ValueKind != JsonValueKind.Null ? v.GetDateTime() : null;

        private static float? GetNullableFloat(JsonElement el, string prop)
            => el.TryGetProperty(prop, out var v) && v.ValueKind != JsonValueKind.Null ? (float)v.GetDouble() : null;

        private static int? GetNullableInt(JsonElement el, string prop)
            => el.TryGetProperty(prop, out var v) && v.ValueKind != JsonValueKind.Null ? v.GetInt32() : null;
    }
}
