using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Repository.Data.DataSeed
{
    public static class PayoutRequestSeed
    {
        public static async Task SeedAsync(TalentreeDbContext context)
        {
            if (context.Set<PayoutRequest>().Any()) return;

            var fashionBoId = BusinessOwnerSeed.FashionBoId;
            var craftsBoId = BusinessOwnerSeed.CraftsBoId;
            var beautyBoId = BusinessOwnerSeed.BeautyBoId;

            // NOTE: In MVP, BOs are spending money (material orders + production requests).
            // Revenue from customer sales is Phase 2. So available balance is negative
            // for all BOs. These payout requests represent Phase 2 test scenarios
            // seeded for Admin workflow testing only.

            var requests = new List<PayoutRequest>
            {
                // ── Fashion BO — Completed payout (historical) ─────────
                new()
                {
                    BusinessOwnerId      = fashionBoId,
                    Amount               = 5000.00m,
                    Currency             = "EGP",
                    Status               = PayoutStatus.Completed,
                    BankName             = "CIB Bank",
                    AccountHolderName    = "Nour El-Sayed",
                    AccountIdentifierEnc = "EG380019000500000000263180002",  // IBAN (not encrypted in seed)
                    RoutingSwiftCode     = "CIBEEGCX",
                    ProcessedAt          = DateTime.UtcNow.AddDays(-20),
                    ProcessedBy          = "seed-admin",
                    CreatedAt            = DateTime.UtcNow.AddDays(-25)
                },
 
                // ── Crafts BO — Rejected payout ────────────────────────
                new()
                {
                    BusinessOwnerId      = craftsBoId,
                    Amount               = 3000.00m,
                    Currency             = "EGP",
                    Status               = PayoutStatus.Rejected,
                    BankName             = "Banque Misr",
                    AccountHolderName    = "Karim Mansour",
                    AccountIdentifierEnc = "EG800002000156789012345180002",
                    RoutingSwiftCode     = "BMISEGCX",
                    RejectionReason      = "Bank account details could not be verified. Please resubmit with correct IBAN.",
                    ProcessedAt          = DateTime.UtcNow.AddDays(-10),
                    ProcessedBy          = "seed-admin",
                    CreatedAt            = DateTime.UtcNow.AddDays(-12)
                },
 
                // ── Beauty BO — Approved (being processed) ─────────────
                new()
                {
                    BusinessOwnerId      = beautyBoId,
                    Amount               = 8000.00m,
                    Currency             = "EGP",
                    Status               = PayoutStatus.Approved,
                    BankName             = "NBE",
                    AccountHolderName    = "Salma Tarek",
                    AccountIdentifierEnc = "EG380019000500000000263180099",
                    RoutingSwiftCode     = "NBEGEGCX",
                    ProcessedAt          = DateTime.UtcNow.AddDays(-2),
                    ProcessedBy          = "seed-admin",
                    CreatedAt            = DateTime.UtcNow.AddDays(-5)
                }
 
                // NOTE: No Pending requests seeded — the unique filtered index
                // allows only one Pending per BO. Adding a Pending here would
                // block the BO from submitting a new one during testing.
            };

            context.Set<PayoutRequest>().AddRange(requests);
            await context.SaveChangesAsync();
        }
    }
}