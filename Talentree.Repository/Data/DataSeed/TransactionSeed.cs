using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Repository.Data.DataSeed
{
    public static class TransactionSeed
    {
        public static async Task SeedAsync(TalentreeDbContext context)
        {
            if (context.Set<Transaction>().Any()) return;

            var fashionBoId = BusinessOwnerSeed.FashionBoId;
            var craftsBoId = BusinessOwnerSeed.CraftsBoId;
            var beautyBoId = BusinessOwnerSeed.BeautyBoId;

            // ══════════════════════════════════════════════════
            // FASHION BO — Nour El-Sayed
            // Matches her 4 material orders + 1 completed production request
            // Running balance starts at 0, goes negative with each spend
            // ══════════════════════════════════════════════════
            var fashionTransactions = new List<Transaction>
            {
                // Material order #1 — Delivered (oldest, 30 days ago)
                new()
                {
                    BusinessOwnerId      = fashionBoId,
                    Type                 = TransactionType.MaterialPurchase,
                    Description          = "Raw material order — Cotton Fabric x20, Thread x30, Buttons x10",
                    Amount               = -1410.00m,
                    BalanceAfter         = -1410.00m,
                    ReferenceId          = 1,
                    ReferenceType        = "MaterialOrder",
                    CreatedAt            = DateTime.UtcNow.AddDays(-30),
                    CreatedBy            = "stripe-webhook"
                },
                // Material order #2 — Shipped (12 days ago)
                new()
                {
                    BusinessOwnerId      = fashionBoId,
                    Type                 = TransactionType.MaterialPurchase,
                    Description          = "Raw material order — Denim Fabric x10, Zippers x50, Faux Leather x5",
                    Amount               = -1700.00m,
                    BalanceAfter         = -3110.00m,
                    ReferenceId          = 2,
                    ReferenceType        = "MaterialOrder",
                    CreatedAt            = DateTime.UtcNow.AddDays(-12),
                    CreatedBy            = "stripe-webhook"
                },
                // Production request — Winter Abayas (completed, 42 days ago — before order seeds)
                new()
                {
                    BusinessOwnerId      = fashionBoId,
                    Type                 = TransactionType.ProductionRequest,
                    Description          = "Production request — Winter Collection: Abayas & Wrap Coats",
                    Amount               = -18500.00m,
                    BalanceAfter         = -21610.00m,
                    ReferenceId          = 1,
                    ReferenceType        = "ProductionRequest",
                    CreatedAt            = DateTime.UtcNow.AddDays(-84),
                    CreatedBy            = "stripe-webhook"
                },
                // Material order #3 — Processing (3 days ago)
                new()
                {
                    BusinessOwnerId      = fashionBoId,
                    Type                 = TransactionType.MaterialPurchase,
                    Description          = "Raw material order — Linen Fabric x15, Elastic x30, Thread x20",
                    Amount               = -1365.00m,
                    BalanceAfter         = -22975.00m,
                    ReferenceId          = 3,
                    ReferenceType        = "MaterialOrder",
                    CreatedAt            = DateTime.UtcNow.AddDays(-3),
                    CreatedBy            = "stripe-webhook"
                }
            };

            // ══════════════════════════════════════════════════
            // CRAFTS BO — Karim Mansour
            // Matches his 3 material orders + 1 completed production request
            // ══════════════════════════════════════════════════
            var craftsTransactions = new List<Transaction>
            {
                // Production request — Scented Candle Set (completed, 55 days ago)
                new()
                {
                    BusinessOwnerId      = craftsBoId,
                    Type                 = TransactionType.ProductionRequest,
                    Description          = "Production request — Scented Candle Set: 500 units",
                    Amount               = -7200.00m,
                    BalanceAfter         = -7200.00m,
                    ReferenceId          = 5,
                    ReferenceType        = "ProductionRequest",
                    CreatedAt            = DateTime.UtcNow.AddDays(-55),
                    CreatedBy            = "stripe-webhook"
                },
                // Material order #5 — Delivered (45 days ago)
                new()
                {
                    BusinessOwnerId      = craftsBoId,
                    Type                 = TransactionType.MaterialPurchase,
                    Description          = "Raw material order — Soy Wax x5, Wicks x10, Fragrance Oil x6, Kraft Boxes x100",
                    Amount               = -1635.00m,
                    BalanceAfter         = -8835.00m,
                    ReferenceId          = 5,
                    ReferenceType        = "MaterialOrder",
                    CreatedAt            = DateTime.UtcNow.AddDays(-45),
                    CreatedBy            = "stripe-webhook"
                },
                // Material order #6 — Refunded (note: amount positive — money returned)
                new()
                {
                    BusinessOwnerId      = craftsBoId,
                    Type                 = TransactionType.Refund,
                    Description          = "Refund — Raw material order #6 (Epoxy Resin Kit x3, Air Dry Clay x4)",
                    Amount               = +500.00m,   // credit — refund received
                    BalanceAfter         = -8335.00m,
                    ReferenceId          = 6,
                    ReferenceType        = "MaterialOrder",
                    CreatedAt            = DateTime.UtcNow.AddDays(-18),
                    CreatedBy            = "stripe-webhook"
                }
            };

            // ══════════════════════════════════════════════════
            // BEAUTY BO — Salma Tarek
            // Matches her 3 material orders + 1 completed + 1 confirmed production request
            // ══════════════════════════════════════════════════
            var beautyTransactions = new List<Transaction>
            {
                // Production request — Shea Body Butter (completed, 70 days ago)
                new()
                {
                    BusinessOwnerId      = beautyBoId,
                    Type                 = TransactionType.ProductionRequest,
                    Description          = "Production request — Shea Body Butter: 300 jars",
                    Amount               = -8400.00m,
                    BalanceAfter         = -8400.00m,
                    ReferenceId          = 9,
                    ReferenceType        = "ProductionRequest",
                    CreatedAt            = DateTime.UtcNow.AddDays(-70),
                    CreatedBy            = "stripe-webhook"
                },
                // Material order #8 — Delivered (60 days ago)
                new()
                {
                    BusinessOwnerId      = beautyBoId,
                    Type                 = TransactionType.MaterialPurchase,
                    Description          = "Raw material order — Shea Butter x5, Coconut Oil x4, Lavender x10, Glass Jars x48",
                    Amount               = -3258.00m,
                    BalanceAfter         = -11658.00m,
                    ReferenceId          = 8,
                    ReferenceType        = "MaterialOrder",
                    CreatedAt            = DateTime.UtcNow.AddDays(-60),
                    CreatedBy            = "stripe-webhook"
                },
                // Material order #9 — Shipped (7 days ago)
                new()
                {
                    BusinessOwnerId      = beautyBoId,
                    Type                 = TransactionType.MaterialPurchase,
                    Description          = "Raw material order — Argan Oil x10, Beeswax x3, Soap Base x6",
                    Amount               = -3130.00m,
                    BalanceAfter         = -14788.00m,
                    ReferenceId          = 9,
                    ReferenceType        = "MaterialOrder",
                    CreatedAt            = DateTime.UtcNow.AddDays(-7),
                    CreatedBy            = "stripe-webhook"
                },
                // Production request — Glycerin Soap Bars confirmed + paid (7 days ago)
                new()
                {
                    BusinessOwnerId      = beautyBoId,
                    Type                 = TransactionType.ProductionRequest,
                    Description          = "Production request — Glycerin Soap Bars: Rose & Honey Collection (400 units)",
                    Amount               = -6200.00m,
                    BalanceAfter         = -20988.00m,
                    ReferenceId          = 10,
                    ReferenceType        = "ProductionRequest",
                    CreatedAt            = DateTime.UtcNow.AddDays(-7),
                    CreatedBy            = "stripe-webhook"
                }
            };

            context.Set<Transaction>().AddRange(fashionTransactions);
            context.Set<Transaction>().AddRange(craftsTransactions);
            context.Set<Transaction>().AddRange(beautyTransactions);
            await context.SaveChangesAsync();
        }
    }
}