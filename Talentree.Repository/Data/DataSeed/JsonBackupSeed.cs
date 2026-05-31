// ============================================================
// Talentree.Repository/Data/DataSeed/JsonBackupSeed.cs
// ============================================================
// Reads seeded_data_backup.json and inserts every section into
// the correct EF DbSet. Each section is idempotent — it checks
// whether any rows already exist before inserting.
//
// Tables covered (in dependency order):
//   1. Transactions
//   2. LoginHistories
//   3. ProductReviews
//   4. SupportTickets
//   5. TicketMessages     (after SupportTickets)
//   6. OnboardingProgress
//   7. PayoutRequests
//   8. BoProductionRequests
// ============================================================
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Talentree.Core.Entities;
using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;

namespace Talentree.Repository.Data.DataSeed
{
    public static class JsonBackupSeed
    {
        public static async Task SeedAsync(TalentreeDbContext context, string jsonFilePath)
        {
            if (!File.Exists(jsonFilePath))
            {
                Console.WriteLine($"[JsonBackupSeed] File not found: {jsonFilePath} — skipping.");
                return;
            }

            await using var stream = File.OpenRead(jsonFilePath);
            using var doc = await JsonDocument.ParseAsync(stream);
            var root = doc.RootElement;

            if (root.TryGetProperty("Transactions", out var txElem))
                await SeedTransactionsAsync(context, txElem);

            if (root.TryGetProperty("LoginHistories", out var lhElem))
                await SeedLoginHistoriesAsync(context, lhElem);

            if (root.TryGetProperty("ProductReviews", out var prElem))
                await SeedProductReviewsAsync(context, prElem);

            if (root.TryGetProperty("SupportTickets", out var stElem))
                await SeedSupportTicketsAsync(context, stElem);

            if (root.TryGetProperty("TicketMessages", out var tmElem))
                await SeedTicketMessagesAsync(context, tmElem);

            if (root.TryGetProperty("OnboardingProgress", out var opElem))
                await SeedOnboardingProgressAsync(context, opElem);

            if (root.TryGetProperty("PayoutRequests", out var pqElem))
                await SeedPayoutRequestsAsync(context, pqElem);

            if (root.TryGetProperty("BoProductionRequests", out var bpElem))
                await SeedBoProductionRequestsAsync(context, bpElem);

            Console.WriteLine("[JsonBackupSeed] ✅ All sections processed.");
        }

        // ──────────────────────────────────────────────────────────────
        // 1. Transactions
        // ReferenceType is string; TransactionType is enum.
        // ──────────────────────────────────────────────────────────────
        private static async Task SeedTransactionsAsync(TalentreeDbContext ctx, JsonElement arr)
        {
            var items = arr.EnumerateArray().ToList();
            if (!items.Any()) return;

            var existingIds = ctx.Set<Transaction>().Select(t => t.Id).ToHashSet();
            var validUserIds = ctx.Set<AppUser>().Select(u => u.Id).ToHashSet();
            var list = new List<Transaction>();
            foreach (var el in items)
            {
                var id = el.GetProperty("Id").GetInt32();
                if (existingIds.Contains(id)) continue;

                var boId = el.GetProperty("BusinessOwnerId").GetString()!;
                if (!validUserIds.Contains(boId)) continue;

                var typeStr = el.GetProperty("Type").GetString() ?? "Sale";

                list.Add(new Transaction
                {
                    Id                    = id,
                    BusinessOwnerId       = boId,
                    Type                  = ParseEnum<TransactionType>(typeStr),
                    Description           = el.GetProperty("Description").GetString() ?? string.Empty,
                    Amount                = el.GetProperty("Amount").GetDecimal(),
                    BalanceAfter          = el.GetProperty("BalanceAfter").GetDecimal(),
                    ReferenceId           = el.GetProperty("ReferenceId").ValueKind == JsonValueKind.Null
                                            ? null : el.GetProperty("ReferenceId").GetInt32(),
                    ReferenceType         = GetNullableString(el, "ReferenceType"),
                    StripePaymentIntentId = GetNullableString(el, "StripePaymentIntentId"),
                    AnomalyFlag           = el.GetProperty("AnomalyFlag").GetBoolean(),
                    AnomalyScore          = GetNullableFloat(el, "AnomalyScore"),
                    CreatedAt             = el.GetProperty("CreatedAt").GetDateTime(),
                    UpdatedAt             = el.GetProperty("UpdatedAt").GetDateTime(),
                    CreatedBy             = el.GetProperty("CreatedBy").GetString() ?? string.Empty,
                    UpdatedBy             = el.GetProperty("UpdatedBy").GetString() ?? string.Empty,
                });
            }

            ctx.Set<Transaction>().AddRange(list);
            await SaveChangesWithIdentityInsertAsync(ctx, "Transactions");
            Console.WriteLine($"[JsonBackupSeed] Transactions → {list.Count} rows.");
        }

        // ──────────────────────────────────────────────────────────────
        // 2. LoginHistories
        // ──────────────────────────────────────────────────────────────
        private static async Task SeedLoginHistoriesAsync(TalentreeDbContext ctx, JsonElement arr)
        {
            var items = arr.EnumerateArray().ToList();
            if (!items.Any()) return;

            var existingIds = ctx.Set<LoginHistory>().Select(x => x.Id).ToHashSet();
            var validUserIds = ctx.Set<AppUser>().Select(u => u.Id).ToHashSet();
            var list = new List<LoginHistory>();
            foreach (var el in items)
            {
                var id = el.GetProperty("Id").GetInt32();
                if (existingIds.Contains(id)) continue;

                var userId = el.GetProperty("UserId").GetString()!;
                if (!validUserIds.Contains(userId)) continue;

                list.Add(new LoginHistory
                {
                    Id           = id,
                    UserId       = userId,
                    IpAddress    = el.GetProperty("IpAddress").GetString() ?? string.Empty,
                    DeviceInfo   = GetNullableString(el, "DeviceInfo"),
                    Location     = GetNullableString(el, "Location"),
                    LoginAt      = el.GetProperty("LoginAt").GetDateTime(),
                    IsSuccessful = el.GetProperty("IsSuccessful").GetBoolean(),
                });
            }

            ctx.Set<LoginHistory>().AddRange(list);
            await SaveChangesWithIdentityInsertAsync(ctx, "LoginHistories");
            Console.WriteLine($"[JsonBackupSeed] LoginHistories → {list.Count} rows.");
        }

        // ──────────────────────────────────────────────────────────────
        // 3. ProductReviews
        // Rating is byte; no Id override needed (EF auto-generates).
        // We use explicit Ids from JSON to preserve backup fidelity.
        // ──────────────────────────────────────────────────────────────
        private static async Task SeedProductReviewsAsync(TalentreeDbContext ctx, JsonElement arr)
        {
            var items = arr.EnumerateArray().ToList();
            if (!items.Any()) return;

            var existingIds = ctx.Set<ProductReview>().Select(x => x.Id).ToHashSet();
            var validProductIds = ctx.Set<Product>().Select(p => p.Id).ToHashSet();
            var validUserIds = ctx.Set<AppUser>().Select(u => u.Id).ToHashSet();

            var list = new List<ProductReview>();
            foreach (var el in items)
            {
                var id = el.GetProperty("Id").GetInt32();
                if (existingIds.Contains(id)) continue;

                var productId = el.GetProperty("ProductId").GetInt32();
                if (!validProductIds.Contains(productId)) continue;

                var customerUserId = el.GetProperty("CustomerUserId").GetString()!;
                if (!validUserIds.Contains(customerUserId)) continue;

                list.Add(new ProductReview
                {
                    Id             = id,
                    ProductId      = productId,
                    CustomerUserId = customerUserId,
                    CustomerName   = el.GetProperty("CustomerName").GetString() ?? string.Empty,
                    Rating         = (byte)el.GetProperty("Rating").GetInt32(),
                    ReviewText     = GetNullableString(el, "ReviewText"),
                    IsAnonymous    = el.GetProperty("IsAnonymous").GetBoolean(),
                    OwnerResponse  = GetNullableString(el, "OwnerResponse"),
                    ResponseAt     = GetNullableDateTime(el, "ResponseAt"),
                    SentimentScore = GetNullableFloat(el, "SentimentScore"),
                    SentimentLabel = GetNullableString(el, "SentimentLabel"),
                    FlaggedToxic   = el.GetProperty("FlaggedToxic").GetBoolean(),
                    CreatedAt      = el.GetProperty("CreatedAt").GetDateTime(),
                    UpdatedAt      = el.GetProperty("UpdatedAt").GetDateTime(),
                    CreatedBy      = el.GetProperty("CreatedBy").GetString() ?? string.Empty,
                    UpdatedBy      = el.GetProperty("UpdatedBy").GetString() ?? string.Empty,
                });
            }

            ctx.Set<ProductReview>().AddRange(list);
            await SaveChangesWithIdentityInsertAsync(ctx, "ProductReviews");
            Console.WriteLine($"[JsonBackupSeed] ProductReviews → {list.Count} rows.");
        }

        // ──────────────────────────────────────────────────────────────
        // 4. SupportTickets
        // Category/Status/Priority stored as integer enum in JSON.
        // ──────────────────────────────────────────────────────────────
        private static async Task SeedSupportTicketsAsync(TalentreeDbContext ctx, JsonElement arr)
        {
            var items = arr.EnumerateArray().ToList();
            if (!items.Any()) return;

            var existingIds = ctx.Set<SupportTicket>().Select(x => x.Id).ToHashSet();
            var validUserIds = ctx.Set<AppUser>().Select(u => u.Id).ToHashSet();
            var list = new List<SupportTicket>();
            foreach (var el in items)
            {
                var id = el.GetProperty("Id").GetInt32();
                if (existingIds.Contains(id)) continue;

                var boUserId = el.GetProperty("BusinessOwnerUserId").GetString()!;
                if (!validUserIds.Contains(boUserId)) continue;

                var assignedToAdminId = GetNullableString(el, "AssignedToAdminId");
                if (assignedToAdminId != null && !validUserIds.Contains(assignedToAdminId)) continue;

                list.Add(new SupportTicket
                {
                    Id                  = id,
                    BusinessOwnerUserId = boUserId,
                    Category            = (TicketCategory)el.GetProperty("Category").GetInt32(),
                    Subject             = el.GetProperty("Subject").GetString() ?? string.Empty,
                    Description         = el.GetProperty("Description").GetString() ?? string.Empty,
                    Status              = (TicketStatus)el.GetProperty("Status").GetInt32(),
                    Priority            = (TicketPriority)el.GetProperty("Priority").GetInt32(),
                    AssignedToAdminId   = assignedToAdminId,
                    AssignedAt          = GetNullableDateTime(el, "AssignedAt"),
                    ClosedAt            = GetNullableDateTime(el, "ClosedAt"),
                    ClosedBy            = GetNullableString(el, "ClosedBy"),
                    ResolvedAt          = GetNullableDateTime(el, "ResolvedAt"),
                    ResolvedBy          = GetNullableString(el, "ResolvedBy"),
                    TicketNumber        = el.GetProperty("TicketNumber").GetString() ?? string.Empty,
                    IsDeleted           = el.GetProperty("IsDeleted").GetBoolean(),
                    CreatedAt           = el.GetProperty("CreatedAt").GetDateTime(),
                    UpdatedAt           = el.GetProperty("UpdatedAt").GetDateTime(),
                    CreatedBy           = el.GetProperty("CreatedBy").GetString() ?? string.Empty,
                    UpdatedBy           = el.GetProperty("UpdatedBy").GetString() ?? string.Empty,
                });
            }

            ctx.Set<SupportTicket>().AddRange(list);
            await SaveChangesWithIdentityInsertAsync(ctx, "SupportTickets");
            Console.WriteLine($"[JsonBackupSeed] SupportTickets → {list.Count} rows.");
        }

        // ──────────────────────────────────────────────────────────────
        // 5. TicketMessages
        // Depends on SupportTickets — filters out orphan messages.
        // ──────────────────────────────────────────────────────────────
        private static async Task SeedTicketMessagesAsync(TalentreeDbContext ctx, JsonElement arr)
        {
            var items = arr.EnumerateArray().ToList();
            if (!items.Any()) return;

            var existingIds = ctx.Set<TicketMessage>().Select(x => x.Id).ToHashSet();
            var validTicketIds = ctx.Set<SupportTicket>().Select(t => t.Id).ToHashSet();
            var validUserIds = ctx.Set<AppUser>().Select(u => u.Id).ToHashSet();

            var list = new List<TicketMessage>();
            foreach (var el in items)
            {
                var id = el.GetProperty("Id").GetInt32();
                if (existingIds.Contains(id)) continue;

                var ticketId = el.GetProperty("TicketId").GetInt32();
                if (!validTicketIds.Contains(ticketId)) continue;

                var senderId = el.GetProperty("SenderId").GetString()!;
                if (!validUserIds.Contains(senderId)) continue;

                list.Add(new TicketMessage
                {
                    Id             = id,
                    TicketId       = ticketId,
                    SenderId       = senderId,
                    Content        = el.GetProperty("Content").GetString() ?? string.Empty,
                    IsAdminMessage = el.GetProperty("IsAdminMessage").GetBoolean(),
                    EmailSent      = el.TryGetProperty("EmailSent", out var es)
                                     && es.ValueKind != JsonValueKind.Null
                                     && es.GetBoolean(),
                    CreatedAt      = el.GetProperty("CreatedAt").GetDateTime(),
                    UpdatedAt      = el.GetProperty("UpdatedAt").GetDateTime(),
                });
            }

            ctx.Set<TicketMessage>().AddRange(list);
            await SaveChangesWithIdentityInsertAsync(ctx, "TicketMessages");
            Console.WriteLine($"[JsonBackupSeed] TicketMessages → {list.Count} rows.");
        }

        // ──────────────────────────────────────────────────────────────
        // 6. OnboardingProgress
        // BaseEntity — has Id but no audit columns.
        // ──────────────────────────────────────────────────────────────
        private static async Task SeedOnboardingProgressAsync(TalentreeDbContext ctx, JsonElement arr)
        {
            var items = arr.EnumerateArray().ToList();
            if (!items.Any()) return;

            var existingIds = ctx.Set<OnboardingProgress>().Select(x => x.Id).ToHashSet();
            var validUserIds = ctx.Set<AppUser>().Select(u => u.Id).ToHashSet();
            var list = new List<OnboardingProgress>();
            foreach (var el in items)
            {
                var id = el.GetProperty("Id").GetInt32();
                if (existingIds.Contains(id)) continue;

                var boId = el.GetProperty("BusinessOwnerId").GetString()!;
                if (!validUserIds.Contains(boId)) continue;

                list.Add(new OnboardingProgress
                {
                    Id                    = id,
                    BusinessOwnerId       = boId,
                    TourCompleted         = el.GetProperty("TourCompleted").GetBoolean(),
                    ChecklistProductAdded = el.GetProperty("ChecklistProductAdded").GetBoolean(),
                    ChecklistPaymentSet   = el.GetProperty("ChecklistPaymentSet").GetBoolean(),
                    ChecklistProfileDone  = el.GetProperty("ChecklistProfileDone").GetBoolean(),
                });
            }

            ctx.Set<OnboardingProgress>().AddRange(list);
            await SaveChangesWithIdentityInsertAsync(ctx, "OnboardingProgress");
            Console.WriteLine($"[JsonBackupSeed] OnboardingProgress → {list.Count} rows.");
        }

        // ──────────────────────────────────────────────────────────────
        // 7. PayoutRequests
        // Status is PayoutStatus enum (string in JSON).
        // ──────────────────────────────────────────────────────────────
        private static async Task SeedPayoutRequestsAsync(TalentreeDbContext ctx, JsonElement arr)
        {
            var items = arr.EnumerateArray().ToList();
            if (!items.Any()) return;

            var existingIds = ctx.Set<PayoutRequest>().Select(x => x.Id).ToHashSet();
            var validUserIds = ctx.Set<AppUser>().Select(u => u.Id).ToHashSet();
            var list = new List<PayoutRequest>();
            foreach (var el in items)
            {
                var id = el.GetProperty("Id").GetInt32();
                if (existingIds.Contains(id)) continue;

                var boId = el.GetProperty("BusinessOwnerId").GetString()!;
                if (!validUserIds.Contains(boId)) continue;

                var statusStr = el.GetProperty("Status").GetString() ?? "Pending";

                list.Add(new PayoutRequest
                {
                    Id                   = id,
                    BusinessOwnerId      = boId,
                    Amount               = el.GetProperty("Amount").GetDecimal(),
                    Currency             = el.GetProperty("Currency").GetString() ?? "EGP",
                    Status               = ParseEnum<PayoutStatus>(statusStr),
                    BankName             = GetNullableString(el, "BankName"),
                    AccountHolderName    = GetNullableString(el, "AccountHolderName"),
                    AccountIdentifierEnc = GetNullableString(el, "AccountIdentifierEnc"),
                    RoutingSwiftCode     = GetNullableString(el, "RoutingSwiftCode"),
                    RejectionReason      = GetNullableString(el, "RejectionReason"),
                    ProcessedAt          = GetNullableDateTime(el, "ProcessedAt"),
                    ProcessedBy          = GetNullableString(el, "ProcessedBy"),
                    CreatedAt            = el.GetProperty("CreatedAt").GetDateTime(),
                    UpdatedAt            = el.GetProperty("UpdatedAt").GetDateTime(),
                    CreatedBy            = el.GetProperty("CreatedBy").GetString() ?? string.Empty,
                    UpdatedBy            = el.GetProperty("UpdatedBy").GetString() ?? string.Empty,
                });
            }

            ctx.Set<PayoutRequest>().AddRange(list);
            await SaveChangesWithIdentityInsertAsync(ctx, "PayoutRequests");
            Console.WriteLine($"[JsonBackupSeed] PayoutRequests → {list.Count} rows.");
        }

        // ──────────────────────────────────────────────────────────────
        // 8. BoProductionRequests
        // Status = BoProductionRequestStatus (string enum in JSON).
        // PaymentStatus = PaymentStatus enum (string "Paid" in JSON).
        // ──────────────────────────────────────────────────────────────
        private static async Task SeedBoProductionRequestsAsync(TalentreeDbContext ctx, JsonElement arr)
        {
            var items = arr.EnumerateArray().ToList();
            if (!items.Any()) return;

            var existingIds = ctx.Set<BoProductionRequest>().Select(x => x.Id).ToHashSet();
            var validUserIds = ctx.Set<AppUser>().Select(u => u.Id).ToHashSet();
            var list = new List<BoProductionRequest>();
            foreach (var el in items)
            {
                var id = el.GetProperty("Id").GetInt32();
                if (existingIds.Contains(id)) continue;

                var boId = el.GetProperty("BusinessOwnerId").GetString()!;
                if (!validUserIds.Contains(boId)) continue;

                var statusStr        = el.GetProperty("Status").GetString() ?? "Submitted";
                var paymentStatusStr = el.GetProperty("PaymentStatus").GetString() ?? "Unpaid";

                list.Add(new BoProductionRequest
                {
                    Id                      = id,
                    BusinessOwnerId         = boId,
                    Title                   = el.GetProperty("Title").GetString() ?? string.Empty,
                    Notes                   = GetNullableString(el, "Notes"),
                    Status                  = ParseEnum<BoProductionRequestStatus>(statusStr),
                    QuotedPrice             = el.GetProperty("QuotedPrice").GetDecimal(),
                    AdminNotes              = GetNullableString(el, "AdminNotes"),
                    EstimatedCompletionDate = GetNullableDateTime(el, "EstimatedCompletionDate"),
                    CompletedAt             = GetNullableDateTime(el, "CompletedAt"),
                    FraudScore              = GetNullableFloat(el, "FraudScore"),
                    FulfillmentTimeHours    = GetNullableInt(el, "FulfillmentTimeHours"),
                    IsFraudFlag             = el.GetProperty("IsFraudFlag").GetBoolean(),
                    PaymentStatus           = ParseEnum<PaymentStatus>(paymentStatusStr),
                    StripePaymentIntentId   = GetNullableString(el, "StripePaymentIntentId"),
                    CreatedAt               = el.GetProperty("CreatedAt").GetDateTime(),
                    UpdatedAt               = el.GetProperty("UpdatedAt").GetDateTime(),
                    CreatedBy               = el.GetProperty("CreatedBy").GetString() ?? string.Empty,
                    UpdatedBy               = el.GetProperty("UpdatedBy").GetString() ?? string.Empty,
                });
            }

            ctx.Set<BoProductionRequest>().AddRange(list);
            await SaveChangesWithIdentityInsertAsync(ctx, "BoProductionRequests");
            Console.WriteLine($"[JsonBackupSeed] BoProductionRequests → {list.Count} rows.");
        }

        // ──────────────────────────────────────────────────────────────
        // Helpers
        // ──────────────────────────────────────────────────────────────
        private static async Task SaveChangesWithIdentityInsertAsync(TalentreeDbContext ctx, string tableName)
        {
            var connection = ctx.Database.GetDbConnection();
            bool wasOpen = connection.State == System.Data.ConnectionState.Open;
            if (!wasOpen) await ctx.Database.OpenConnectionAsync();

            await using var transaction = await ctx.Database.BeginTransactionAsync();
            try
            {
#pragma warning disable EF1002
                await ctx.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT [dbo].[{tableName}] ON");
                await ctx.SaveChangesAsync();
                await ctx.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT [dbo].[{tableName}] OFF");
#pragma warning restore EF1002
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
            finally
            {
                if (!wasOpen) await ctx.Database.CloseConnectionAsync();
            }
        }

        private static T ParseEnum<T>(string value) where T : struct, Enum
        {
            return Enum.TryParse<T>(value, ignoreCase: true, out var result) ? result : default;
        }

        private static string? GetNullableString(JsonElement el, string prop)
        {
            return el.TryGetProperty(prop, out var v) && v.ValueKind != JsonValueKind.Null
                   ? v.GetString() : null;
        }

        private static DateTime? GetNullableDateTime(JsonElement el, string prop)
        {
            return el.TryGetProperty(prop, out var v) && v.ValueKind != JsonValueKind.Null
                   ? v.GetDateTime() : null;
        }

        private static float? GetNullableFloat(JsonElement el, string prop)
        {
            return el.TryGetProperty(prop, out var v) && v.ValueKind != JsonValueKind.Null
                   ? (float)v.GetDouble() : null;
        }

        private static int? GetNullableInt(JsonElement el, string prop)
        {
            return el.TryGetProperty(prop, out var v) && v.ValueKind != JsonValueKind.Null
                   ? v.GetInt32() : null;
        }
    }
}
