// ============================================================
// Talentree.Repository/Data/DataSeed/JsonSeedLoader.cs
// ============================================================
// Reads individual JSON files from the jsonSeed/ folder and
// inserts or updates data into the correct EF DbSets.
//
// Execution order (dependency-safe):
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
using Microsoft.EntityFrameworkCore;
using Talentree.Core.Entities;
using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;

namespace Talentree.Repository.Data.DataSeed
{
    public static class JsonSeedLoader
    {
        public static async Task SeedAsync(TalentreeDbContext context, string jsonSeedFolderPath)
        {
            if (!Directory.Exists(jsonSeedFolderPath))
            {
                Console.WriteLine($"[JsonSeedLoader] Folder not found: {jsonSeedFolderPath} — skipping.");
                return;
            }

            Console.WriteLine($"[JsonSeedLoader] Loading from: {jsonSeedFolderPath}");

            // ── 1. Transactions ──────────────────────────────────────────
            await SeedTransactionsAsync(context, Path.Combine(jsonSeedFolderPath, "Transactions.json"));

            // ── 2. LoginHistories ────────────────────────────────────────
            await SeedLoginHistoriesAsync(context, Path.Combine(jsonSeedFolderPath, "LoginHistories.json"));

            // ── 3. ProductReviews ────────────────────────────────────────
            await SeedProductReviewsAsync(context, Path.Combine(jsonSeedFolderPath, "ProductReviews.json"));

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
            await UpdateProductStatsAsync(context, Path.Combine(jsonSeedFolderPath, "Products_stats_update.json"));

            Console.WriteLine("[JsonSeedLoader] ✅ All sections processed.");
        }

        // ──────────────────────────────────────────────────────────────
        // 1. Transactions
        // ──────────────────────────────────────────────────────────────
        private static async Task SeedTransactionsAsync(TalentreeDbContext ctx, string filePath)
        {
            if (!File.Exists(filePath)) { Console.WriteLine($"[JsonSeedLoader] {Path.GetFileName(filePath)} not found — skipping."); return; }
            if (await ctx.Set<Transaction>().AnyAsync(t => t.StripePaymentIntentId == "pi_vbdivuzzpqk51fpkh1dnmmjb"))
            { Console.WriteLine("[JsonSeedLoader] Transactions JSON already seeded — skipping."); return; }

            using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(filePath));
            var validUserIds = ctx.Set<AppUser>().Select(u => u.Id).ToHashSet();
            var list = new List<Transaction>();

            foreach (var el in doc.RootElement.EnumerateArray())
            {
                var boId = el.GetProperty("BusinessOwnerId").GetString()!;
                if (!validUserIds.Contains(boId)) continue;

                list.Add(new Transaction
                {
                    BusinessOwnerId       = boId,
                    Type                  = ParseEnum<TransactionType>(el.GetProperty("Type").GetString() ?? "Sale"),
                    Description           = el.GetProperty("Description").GetString() ?? string.Empty,
                    Amount                = el.GetProperty("Amount").GetDecimal(),
                    BalanceAfter          = el.GetProperty("BalanceAfter").GetDecimal(),
                    ReferenceId           = GetNullableInt(el, "ReferenceId"),
                    ReferenceType         = GetNullableString(el, "ReferenceType"),
                    StripePaymentIntentId = GetNullableString(el, "StripePaymentIntentId"),
                    AnomalyFlag           = el.GetProperty("AnomalyFlag").GetInt32() != 0,
                    AnomalyScore          = GetNullableFloat(el, "AnomalyScore"),
                    CreatedAt             = el.GetProperty("CreatedAt").GetDateTime(),
                    UpdatedAt             = el.GetProperty("UpdatedAt").GetDateTime(),
                    CreatedBy             = el.GetProperty("CreatedBy").GetString() ?? boId,
                    UpdatedBy             = el.GetProperty("UpdatedBy").GetString() ?? boId,
                });
            }

            ctx.Set<Transaction>().AddRange(list);
            await ctx.SaveChangesAsync();
            Console.WriteLine($"[JsonSeedLoader] Transactions → {list.Count} rows inserted.");
        }

        // ──────────────────────────────────────────────────────────────
        // 2. LoginHistories
        // ──────────────────────────────────────────────────────────────
        private static async Task SeedLoginHistoriesAsync(TalentreeDbContext ctx, string filePath)
        {
            if (!File.Exists(filePath)) { Console.WriteLine($"[JsonSeedLoader] {Path.GetFileName(filePath)} not found — skipping."); return; }
            if (await ctx.Set<LoginHistory>().AnyAsync(l => l.IpAddress == "192.168.200.189" && l.DeviceInfo == "Dell XPS"))
            { Console.WriteLine("[JsonSeedLoader] LoginHistories JSON already seeded — skipping."); return; }

            using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(filePath));
            var validUserIds = ctx.Set<AppUser>().Select(u => u.Id).ToHashSet();
            var list = new List<LoginHistory>();

            foreach (var el in doc.RootElement.EnumerateArray())
            {
                var userId = el.GetProperty("UserId").GetString()!;
                if (!validUserIds.Contains(userId)) continue;

                list.Add(new LoginHistory
                {
                    UserId       = userId,
                    IpAddress    = el.GetProperty("IpAddress").GetString() ?? string.Empty,
                    DeviceInfo   = GetNullableString(el, "DeviceInfo"),
                    Location     = GetNullableString(el, "Location"),
                    LoginAt      = el.GetProperty("LoginAt").GetDateTime(),
                    IsSuccessful = el.GetProperty("IsSuccessful").GetInt32() != 0,
                });
            }

            ctx.Set<LoginHistory>().AddRange(list);
            await ctx.SaveChangesAsync();
            Console.WriteLine($"[JsonSeedLoader] LoginHistories → {list.Count} rows inserted.");
        }

        // ──────────────────────────────────────────────────────────────
        // 3. ProductReviews
        // ──────────────────────────────────────────────────────────────
        private static async Task SeedProductReviewsAsync(TalentreeDbContext ctx, string filePath)
        {
            if (!File.Exists(filePath)) { Console.WriteLine($"[JsonSeedLoader] {Path.GetFileName(filePath)} not found — skipping."); return; }
            if (await ctx.Set<ProductReview>().AnyAsync(r => r.CustomerName == "User b140"))
            { Console.WriteLine("[JsonSeedLoader] ProductReviews JSON already seeded — skipping."); return; }

            using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(filePath));
            var validProductIds = ctx.Set<Product>().Select(p => p.Id).ToHashSet();
            var validUserIds    = ctx.Set<AppUser>().Select(u => u.Id).ToHashSet();
            var list = new List<ProductReview>();

            foreach (var el in doc.RootElement.EnumerateArray())
            {
                var productId = el.GetProperty("ProductId").GetInt32();
                if (!validProductIds.Contains(productId)) continue;

                var customerUserId = el.GetProperty("CustomerUserId").GetString()!;
                if (!validUserIds.Contains(customerUserId)) continue;

                list.Add(new ProductReview
                {
                    ProductId      = productId,
                    CustomerUserId = customerUserId,
                    CustomerName   = el.GetProperty("CustomerName").GetString() ?? string.Empty,
                    Rating         = (byte)el.GetProperty("Rating").GetInt32(),
                    ReviewText     = GetNullableString(el, "ReviewText"),
                    IsAnonymous    = GetIntBool(el, "IsAnonymous"),
                    OwnerResponse  = GetNullableString(el, "OwnerResponse"),
                    ResponseAt     = GetNullableDateTime(el, "ResponseAt"),
                    SentimentScore = GetNullableFloat(el, "SentimentScore"),
                    SentimentLabel = GetNullableString(el, "SentimentLabel"),
                    FlaggedToxic   = GetIntBool(el, "FlaggedToxic"),
                    CreatedAt      = el.GetProperty("CreatedAt").GetDateTime(),
                    UpdatedAt      = el.GetProperty("UpdatedAt").GetDateTime(),
                    CreatedBy      = el.GetProperty("CreatedBy").GetString() ?? customerUserId,
                    UpdatedBy      = el.GetProperty("UpdatedBy").GetString() ?? customerUserId,
                });
            }

            ctx.Set<ProductReview>().AddRange(list);
            await ctx.SaveChangesAsync();
            Console.WriteLine($"[JsonSeedLoader] ProductReviews → {list.Count} rows inserted.");
        }

        // ──────────────────────────────────────────────────────────────
        // 4. SupportTickets
        // Returns the ordered list of auto-generated IDs so that
        // TicketMessages can resolve <<SupportTickets[N].Id>> placeholders.
        // ──────────────────────────────────────────────────────────────
        private static async Task<List<int>> SeedSupportTicketsAsync(TalentreeDbContext ctx, string filePath)
        {
            if (!File.Exists(filePath)) { Console.WriteLine($"[JsonSeedLoader] {Path.GetFileName(filePath)} not found — skipping."); return new List<int>(); }

            // If tickets from JSON already exist, read their IDs in creation order for TicketMessages
            if (await ctx.Set<SupportTicket>().AnyAsync(t => t.TicketNumber == "TKT-15847"))
            {
                Console.WriteLine("[JsonSeedLoader] SupportTickets JSON already seeded — reading existing IDs.");
                return await ctx.Set<SupportTicket>().OrderBy(t => t.Id).Select(t => t.Id).ToListAsync();
            }

            using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(filePath));
            var validUserIds = ctx.Set<AppUser>().Select(u => u.Id).ToHashSet();
            var list = new List<SupportTicket>();

            foreach (var el in doc.RootElement.EnumerateArray())
            {
                var boUserId = el.GetProperty("BusinessOwnerUserId").GetString()!;
                if (!validUserIds.Contains(boUserId)) continue;

                list.Add(new SupportTicket
                {
                    BusinessOwnerUserId = boUserId,
                    Category            = (TicketCategory)el.GetProperty("Category").GetInt32(),
                    Subject             = el.GetProperty("Subject").GetString() ?? string.Empty,
                    Description         = el.GetProperty("Description").GetString() ?? string.Empty,
                    Status              = (TicketStatus)el.GetProperty("Status").GetInt32(),
                    Priority            = (TicketPriority)el.GetProperty("Priority").GetInt32(),
                    TicketNumber        = el.GetProperty("TicketNumber").GetString() ?? string.Empty,
                    IsDeleted           = GetIntBool(el, "IsDeleted"),
                    CreatedAt           = el.GetProperty("CreatedAt").GetDateTime(),
                    UpdatedAt           = el.GetProperty("UpdatedAt").GetDateTime(),
                    CreatedBy           = el.GetProperty("CreatedBy").GetString() ?? boUserId,
                    UpdatedBy           = el.GetProperty("UpdatedBy").GetString() ?? boUserId,
                });
            }

            ctx.Set<SupportTicket>().AddRange(list);
            await ctx.SaveChangesAsync();
            Console.WriteLine($"[JsonSeedLoader] SupportTickets → {list.Count} rows inserted.");

            // Return IDs in insertion order (EF populates Id after SaveChanges)
            return list.Select(t => t.Id).ToList();
        }

        // ──────────────────────────────────────────────────────────────
        // 5. TicketMessages
        // TicketId field contains "<<SupportTickets[N].Id>>" (1-based index).
        // We resolve this using the ordered list returned from step 4.
        // ──────────────────────────────────────────────────────────────
        private static async Task SeedTicketMessagesAsync(TalentreeDbContext ctx, string filePath, List<int> ticketIds)
        {
            if (!File.Exists(filePath)) { Console.WriteLine($"[JsonSeedLoader] {Path.GetFileName(filePath)} not found — skipping."); return; }
            if (await ctx.Set<TicketMessage>().AnyAsync(m => m.Content == "Please see details."))
            { Console.WriteLine("[JsonSeedLoader] TicketMessages JSON already seeded — skipping."); return; }
            if (ticketIds.Count == 0) { Console.WriteLine("[JsonSeedLoader] No ticket IDs available — skipping TicketMessages."); return; }

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

                list.Add(new TicketMessage
                {
                    TicketId       = resolvedTicketId,
                    SenderId       = senderId,
                    Content        = el.GetProperty("Content").GetString() ?? string.Empty,
                    IsAdminMessage = GetIntBool(el, "IsAdminMessage"),
                    EmailSent      = GetIntBool(el, "EmailSent"),
                    CreatedAt      = el.GetProperty("CreatedAt").GetDateTime(),
                    UpdatedAt      = el.GetProperty("UpdatedAt").GetDateTime(),
                });
            }

            ctx.Set<TicketMessage>().AddRange(list);
            await ctx.SaveChangesAsync();
            Console.WriteLine($"[JsonSeedLoader] TicketMessages → {list.Count} rows inserted ({skipped} skipped).");
        }

        // ──────────────────────────────────────────────────────────────
        // 6. OnboardingProgress
        // ──────────────────────────────────────────────────────────────
        private static async Task SeedOnboardingProgressAsync(TalentreeDbContext ctx, string filePath)
        {
            if (!File.Exists(filePath)) { Console.WriteLine($"[JsonSeedLoader] {Path.GetFileName(filePath)} not found — skipping."); return; }
            if (await ctx.Set<OnboardingProgress>().AnyAsync(o => o.BusinessOwnerId == "11111111-1111-1111-1111-111111111101"))
            { Console.WriteLine("[JsonSeedLoader] OnboardingProgress JSON already seeded — skipping."); return; }

            using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(filePath));
            var validUserIds = ctx.Set<AppUser>().Select(u => u.Id).ToHashSet();
            var list = new List<OnboardingProgress>();

            foreach (var el in doc.RootElement.EnumerateArray())
            {
                var boId = el.GetProperty("BusinessOwnerId").GetString()!;
                if (!validUserIds.Contains(boId)) continue;

                list.Add(new OnboardingProgress
                {
                    BusinessOwnerId       = boId,
                    TourCompleted         = GetIntBool(el, "TourCompleted"),
                    ChecklistProductAdded = GetIntBool(el, "ChecklistProductAdded"),
                    ChecklistPaymentSet   = GetIntBool(el, "ChecklistPaymentSet"),
                    ChecklistProfileDone  = GetIntBool(el, "ChecklistProfileDone"),
                });
            }

            ctx.Set<OnboardingProgress>().AddRange(list);
            await ctx.SaveChangesAsync();
            Console.WriteLine($"[JsonSeedLoader] OnboardingProgress → {list.Count} rows inserted.");
        }

        // ──────────────────────────────────────────────────────────────
        // 7. PayoutRequests
        // ──────────────────────────────────────────────────────────────
        private static async Task SeedPayoutRequestsAsync(TalentreeDbContext ctx, string filePath)
        {
            if (!File.Exists(filePath)) { Console.WriteLine($"[JsonSeedLoader] {Path.GetFileName(filePath)} not found — skipping."); return; }
            if (await ctx.Set<PayoutRequest>().AnyAsync(p => p.AccountIdentifierEnc == "ENC_986996619"))
            { Console.WriteLine("[JsonSeedLoader] PayoutRequests JSON already seeded — skipping."); return; }

            using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(filePath));
            var validUserIds = ctx.Set<AppUser>().Select(u => u.Id).ToHashSet();
            var list = new List<PayoutRequest>();

            foreach (var el in doc.RootElement.EnumerateArray())
            {
                var boId = el.GetProperty("BusinessOwnerId").GetString()!;
                if (!validUserIds.Contains(boId)) continue;

                list.Add(new PayoutRequest
                {
                    BusinessOwnerId      = boId,
                    Amount               = el.GetProperty("Amount").GetDecimal(),
                    Currency             = el.GetProperty("Currency").GetString() ?? "EGP",
                    Status               = ParseEnum<PayoutStatus>(el.GetProperty("Status").GetString() ?? "Pending"),
                    BankName             = GetNullableString(el, "BankName"),
                    AccountHolderName    = GetNullableString(el, "AccountHolderName"),
                    AccountIdentifierEnc = GetNullableString(el, "AccountIdentifierEnc"),
                    RoutingSwiftCode     = GetNullableString(el, "RoutingSwiftCode"),
                    RejectionReason      = GetNullableString(el, "RejectionReason"),
                    ProcessedAt          = GetNullableDateTime(el, "ProcessedAt"),
                    ProcessedBy          = GetNullableString(el, "ProcessedBy"),
                    CreatedAt            = el.GetProperty("CreatedAt").GetDateTime(),
                    UpdatedAt            = el.GetProperty("UpdatedAt").GetDateTime(),
                    CreatedBy            = el.GetProperty("CreatedBy").GetString() ?? boId,
                    UpdatedBy            = el.GetProperty("UpdatedBy").GetString() ?? boId,
                });
            }

            ctx.Set<PayoutRequest>().AddRange(list);
            await ctx.SaveChangesAsync();
            Console.WriteLine($"[JsonSeedLoader] PayoutRequests → {list.Count} rows inserted.");
        }

        // ──────────────────────────────────────────────────────────────
        // 8. BoProductionRequests
        // ──────────────────────────────────────────────────────────────
        private static async Task SeedBoProductionRequestsAsync(TalentreeDbContext ctx, string filePath)
        {
            if (!File.Exists(filePath)) { Console.WriteLine($"[JsonSeedLoader] {Path.GetFileName(filePath)} not found — skipping."); return; }
            if (await ctx.Set<BoProductionRequest>().AnyAsync(r => r.Title == "Request 0"))
            { Console.WriteLine("[JsonSeedLoader] BoProductionRequests JSON already seeded — skipping."); return; }

            using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(filePath));
            var validUserIds = ctx.Set<AppUser>().Select(u => u.Id).ToHashSet();
            var list = new List<BoProductionRequest>();

            foreach (var el in doc.RootElement.EnumerateArray())
            {
                var boId = el.GetProperty("BusinessOwnerId").GetString()!;
                if (!validUserIds.Contains(boId)) continue;

                list.Add(new BoProductionRequest
                {
                    BusinessOwnerId         = boId,
                    Title                   = el.GetProperty("Title").GetString() ?? string.Empty,
                    Notes                   = GetNullableString(el, "Notes"),
                    Status                  = ParseEnum<BoProductionRequestStatus>(el.GetProperty("Status").GetString() ?? "Submitted"),
                    QuotedPrice             = el.GetProperty("QuotedPrice").GetDecimal(),
                    AdminNotes              = GetNullableString(el, "AdminNotes"),
                    EstimatedCompletionDate = GetNullableDateTime(el, "EstimatedCompletionDate"),
                    CompletedAt             = GetNullableDateTime(el, "CompletedAt"),
                    FraudScore              = GetNullableFloat(el, "FraudScore"),
                    FulfillmentTimeHours    = GetNullableInt(el, "FulfillmentTimeHours"),
                    IsFraudFlag             = GetIntBool(el, "IsFraudFlag"),
                    PaymentStatus           = ParseEnum<PaymentStatus>(el.GetProperty("PaymentStatus").GetString() ?? "Unpaid"),
                    StripePaymentIntentId   = GetNullableString(el, "StripePaymentIntentId"),
                    CreatedAt               = el.GetProperty("CreatedAt").GetDateTime(),
                    UpdatedAt               = el.GetProperty("UpdatedAt").GetDateTime(),
                    CreatedBy               = el.GetProperty("CreatedBy").GetString() ?? boId,
                    UpdatedBy               = el.GetProperty("UpdatedBy").GetString() ?? boId,
                });
            }

            ctx.Set<BoProductionRequest>().AddRange(list);
            await ctx.SaveChangesAsync();
            Console.WriteLine($"[JsonSeedLoader] BoProductionRequests → {list.Count} rows inserted.");
        }

        // ──────────────────────────────────────────────────────────────
        // 9. Products_stats_update — UPDATE only, not INSERT
        // Patches ViewCount, CartAddCount, PurchaseCount, RevenueTotal,
        // AvgRating on existing Product rows identified by Id.
        // ──────────────────────────────────────────────────────────────
        private static async Task UpdateProductStatsAsync(TalentreeDbContext ctx, string filePath)
        {
            if (!File.Exists(filePath)) { Console.WriteLine($"[JsonSeedLoader] {Path.GetFileName(filePath)} not found — skipping."); return; }

            using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(filePath));
            var updated = 0;

            foreach (var el in doc.RootElement.EnumerateArray())
            {
                var id = el.GetProperty("Id").GetInt32();
                var product = await ctx.Set<Product>().FindAsync(id);
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
        // Helpers
        // ──────────────────────────────────────────────────────────────
        private static T ParseEnum<T>(string value) where T : struct, Enum
            => Enum.TryParse<T>(value, ignoreCase: true, out var r) ? r : default;

        /// <summary>Reads a JSON int field as bool (0 = false, non-zero = true).</summary>
        private static bool GetIntBool(JsonElement el, string prop)
            => el.TryGetProperty(prop, out var v) && v.ValueKind != JsonValueKind.Null && v.GetInt32() != 0;

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
