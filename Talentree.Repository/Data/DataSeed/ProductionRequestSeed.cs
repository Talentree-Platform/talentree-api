using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Repository.Data.DataSeed
{
    public static class ProductionRequestSeed
    {
        private const string AdminId = "seed-admin";

        public static async Task SeedAsync(TalentreeDbContext context)
        {
            if (context.Set<BoProductionRequest>().Any()) return;

            // ── Load raw materials for preferred material references ──
            var cotton = context.Set<RawMaterial>().First(r => r.Name == "Cotton Fabric");
            var denim = context.Set<RawMaterial>().First(r => r.Name == "Denim Fabric");
            var fauxLeather = context.Set<RawMaterial>().First(r => r.Name == "Faux Leather Sheet");
            var soyWax = context.Set<RawMaterial>().First(r => r.Name == "Soy Wax Flakes");
            var resin = context.Set<RawMaterial>().First(r => r.Name == "Epoxy Resin Kit (A+B)");
            var macrame = context.Set<RawMaterial>().First(r => r.Name == "Macramé Cord (3mm)");
            var sheaButter = context.Set<RawMaterial>().First(r => r.Name == "Shea Butter (Raw)");
            var soapBase = context.Set<RawMaterial>().First(r => r.Name == "Soap Base (Melt & Pour)");

            var fashionBoId = BusinessOwnerSeed.FashionBoId;
            var craftsBoId = BusinessOwnerSeed.CraftsBoId;
            var beautyBoId = BusinessOwnerSeed.BeautyBoId;

            var requests = new List<BoProductionRequest>
            {
                // ══════════════════════════════════════════════════
                // FASHION BO — Nour El-Sayed
                // ══════════════════════════════════════════════════
 
                // 1 — Completed (full happy path, oldest)
                new()
                {
                    BusinessOwnerId         = fashionBoId,
                    Title                   = "Winter Collection — Abayas & Wrap Coats",
                    Notes                   = "Need 150 abayas and 80 wrap coats in muted tones. Prefer cotton lining.",
                    Status                  = BoProductionRequestStatus.Completed,
                    QuotedPrice             = 18500.00m,
                    AdminNotes              = "Completed on schedule. Goods dispatched to BO warehouse.",
                    EstimatedCompletionDate = DateTime.UtcNow.AddDays(-45),
                    CompletedAt             = DateTime.UtcNow.AddDays(-42),
                    CreatedAt               = DateTime.UtcNow.AddDays(-90),
                    Items = new List<BoProductionRequestItem>
                    {
                        new() { ProductType = "Abaya",      Quantity = 150, PreferredRawMaterialId = cotton.Id,  Specifications = "Black, navy, dark grey — sizes S/M/L" },
                        new() { ProductType = "Wrap Coat",  Quantity = 80,  PreferredRawMaterialId = cotton.Id,  Specifications = "Camel and dusty rose, sizes M/L/XL" }
                    },
                    StatusHistory = new List<BoProductionRequestStatusHistory>
                    {
                        new() { Status = BoProductionRequestStatus.Submitted,    ChangedByUserId = fashionBoId, Notes = "Request submitted.",                               CreatedAt = DateTime.UtcNow.AddDays(-90) },
                        new() { Status = BoProductionRequestStatus.UnderReview,  ChangedByUserId = AdminId,     Notes = "Under review by Talentree team.",                  CreatedAt = DateTime.UtcNow.AddDays(-88) },
                        new() { Status = BoProductionRequestStatus.Quoted,       ChangedByUserId = AdminId,     Notes = "Quote sent: 18,500 EGP. ETA 45 days.",            CreatedAt = DateTime.UtcNow.AddDays(-86) },
                        new() { Status = BoProductionRequestStatus.Confirmed,    ChangedByUserId = fashionBoId, Notes = "Quote accepted.",                                  CreatedAt = DateTime.UtcNow.AddDays(-84) },
                        new() { Status = BoProductionRequestStatus.InProduction, ChangedByUserId = AdminId,     Notes = "Production started.",                             CreatedAt = DateTime.UtcNow.AddDays(-83) },
                        new() { Status = BoProductionRequestStatus.Completed,    ChangedByUserId = AdminId,     Notes = "Completed on schedule. Ready for collection.",    CreatedAt = DateTime.UtcNow.AddDays(-42) }
                    }
                },
 
                // 2 — InProduction
                new()
                {
                    BusinessOwnerId         = fashionBoId,
                    Title                   = "Spring Line — Linen Dresses & Tote Bags",
                    Notes                   = "200 dresses and 300 tote bags. Totes should have inside pocket.",
                    Status                  = BoProductionRequestStatus.InProduction,
                    QuotedPrice             = 22000.00m,
                    AdminNotes              = "Production started. Estimated 3 weeks remaining.",
                    EstimatedCompletionDate = DateTime.UtcNow.AddDays(21),
                    CompletedAt             = null,
                    CreatedAt               = DateTime.UtcNow.AddDays(-30),
                    Items = new List<BoProductionRequestItem>
                    {
                        new() { ProductType = "Linen Dress", Quantity = 200, PreferredRawMaterialId = null,       Specifications = "Sizes XS-XL, pastel colours" },
                        new() { ProductType = "Tote Bag",    Quantity = 300, PreferredRawMaterialId = cotton.Id,  Specifications = "Natural cotton, 40x35cm with inside pocket" }
                    },
                    StatusHistory = new List<BoProductionRequestStatusHistory>
                    {
                        new() { Status = BoProductionRequestStatus.Submitted,    ChangedByUserId = fashionBoId, Notes = "Request submitted.",          CreatedAt = DateTime.UtcNow.AddDays(-30) },
                        new() { Status = BoProductionRequestStatus.UnderReview,  ChangedByUserId = AdminId,     Notes = "Under review.",               CreatedAt = DateTime.UtcNow.AddDays(-28) },
                        new() { Status = BoProductionRequestStatus.Quoted,       ChangedByUserId = AdminId,     Notes = "Quote: 22,000 EGP.",          CreatedAt = DateTime.UtcNow.AddDays(-26) },
                        new() { Status = BoProductionRequestStatus.Confirmed,    ChangedByUserId = fashionBoId, Notes = "Confirmed.",                  CreatedAt = DateTime.UtcNow.AddDays(-25) },
                        new() { Status = BoProductionRequestStatus.InProduction, ChangedByUserId = AdminId,     Notes = "Production started.",         CreatedAt = DateTime.UtcNow.AddDays(-24) }
                    }
                },
 
                // 3 — Quoted (waiting for BO to confirm)
                new()
                {
                    BusinessOwnerId         = fashionBoId,
                    Title                   = "Denim Jackets — Limited Run",
                    Notes                   = "50 oversized denim jackets. Vintage wash finish preferred.",
                    Status                  = BoProductionRequestStatus.Quoted,
                    QuotedPrice             = 9750.00m,
                    AdminNotes              = "Quote includes vintage wash treatment. Please confirm to proceed.",
                    EstimatedCompletionDate = DateTime.UtcNow.AddDays(30),
                    CompletedAt             = null,
                    CreatedAt               = DateTime.UtcNow.AddDays(-10),
                    Items = new List<BoProductionRequestItem>
                    {
                        new() { ProductType = "Denim Jacket", Quantity = 50, PreferredRawMaterialId = denim.Id, Specifications = "Oversized fit, sizes S-XXL, vintage wash" }
                    },
                    StatusHistory = new List<BoProductionRequestStatusHistory>
                    {
                        new() { Status = BoProductionRequestStatus.Submitted,   ChangedByUserId = fashionBoId, Notes = "Request submitted.",   CreatedAt = DateTime.UtcNow.AddDays(-10) },
                        new() { Status = BoProductionRequestStatus.UnderReview, ChangedByUserId = AdminId,     Notes = "Under review.",        CreatedAt = DateTime.UtcNow.AddDays(-8)  },
                        new() { Status = BoProductionRequestStatus.Quoted,      ChangedByUserId = AdminId,     Notes = "Quote sent: 9,750 EGP. Awaiting confirmation.", CreatedAt = DateTime.UtcNow.AddDays(-6) }
                    }
                },
 
                // 4 — Submitted (just arrived, not reviewed yet)
                new()
                {
                    BusinessOwnerId         = fashionBoId,
                    Title                   = "Leather Crossbody Bags — Eid Collection",
                    Notes                   = "100 crossbody bags in faux leather. Gold hardware preferred.",
                    Status                  = BoProductionRequestStatus.Submitted,
                    QuotedPrice             = null,
                    AdminNotes              = null,
                    EstimatedCompletionDate = null,
                    CompletedAt             = null,
                    CreatedAt               = DateTime.UtcNow.AddDays(-1),
                    Items = new List<BoProductionRequestItem>
                    {
                        new() { ProductType = "Crossbody Bag", Quantity = 100, PreferredRawMaterialId = fauxLeather.Id, Specifications = "Black and tan, 25x18cm, gold hardware" }
                    },
                    StatusHistory = new List<BoProductionRequestStatusHistory>
                    {
                        new() { Status = BoProductionRequestStatus.Submitted, ChangedByUserId = fashionBoId, Notes = "Request submitted.", CreatedAt = DateTime.UtcNow.AddDays(-1) }
                    }
                },
 
                // ══════════════════════════════════════════════════
                // CRAFTS BO — Karim Mansour
                // ══════════════════════════════════════════════════
 
                // 5 — Completed
                new()
                {
                    BusinessOwnerId         = craftsBoId,
                    Title                   = "Scented Candle Set — 500 Units",
                    Notes                   = "Soy wax candles in glass jars. 5 scents: oud, jasmine, vanilla, cedar, rose.",
                    Status                  = BoProductionRequestStatus.Completed,
                    QuotedPrice             = 7200.00m,
                    AdminNotes              = "All 500 units completed. Fragrance ratios applied as requested.",
                    EstimatedCompletionDate = DateTime.UtcNow.AddDays(-20),
                    CompletedAt             = DateTime.UtcNow.AddDays(-18),
                    CreatedAt               = DateTime.UtcNow.AddDays(-60),
                    Items = new List<BoProductionRequestItem>
                    {
                        new() { ProductType = "Scented Candle", Quantity = 500, PreferredRawMaterialId = soyWax.Id, Specifications = "100g soy wax in 50ml glass jars. 5 scents, 100 units each." }
                    },
                    StatusHistory = new List<BoProductionRequestStatusHistory>
                    {
                        new() { Status = BoProductionRequestStatus.Submitted,    ChangedByUserId = craftsBoId, Notes = "Request submitted.",                                 CreatedAt = DateTime.UtcNow.AddDays(-60) },
                        new() { Status = BoProductionRequestStatus.UnderReview,  ChangedByUserId = AdminId,    Notes = "Under review.",                                     CreatedAt = DateTime.UtcNow.AddDays(-58) },
                        new() { Status = BoProductionRequestStatus.Quoted,       ChangedByUserId = AdminId,    Notes = "Quote: 7,200 EGP for 500 units.",                   CreatedAt = DateTime.UtcNow.AddDays(-56) },
                        new() { Status = BoProductionRequestStatus.Confirmed,    ChangedByUserId = craftsBoId, Notes = "Quote confirmed.",                                  CreatedAt = DateTime.UtcNow.AddDays(-55) },
                        new() { Status = BoProductionRequestStatus.InProduction, ChangedByUserId = AdminId,    Notes = "Production started.",                               CreatedAt = DateTime.UtcNow.AddDays(-54) },
                        new() { Status = BoProductionRequestStatus.Completed,    ChangedByUserId = AdminId,    Notes = "All 500 units ready for collection.",               CreatedAt = DateTime.UtcNow.AddDays(-18) }
                    }
                },
 
                // 6 — Rejected
                new()
                {
                    BusinessOwnerId         = craftsBoId,
                    Title                   = "Resin Wall Art Panels — Large Format",
                    Notes                   = "20 large resin wall art panels, 60x90cm each.",
                    Status                  = BoProductionRequestStatus.Rejected,
                    QuotedPrice             = null,
                    AdminNotes              = "We currently do not have the capacity for large-format resin panels. Please resubmit for smaller sizes (max 30x40cm).",
                    EstimatedCompletionDate = null,
                    CompletedAt             = null,
                    CreatedAt               = DateTime.UtcNow.AddDays(-25),
                    Items = new List<BoProductionRequestItem>
                    {
                        new() { ProductType = "Resin Wall Art Panel", Quantity = 20, PreferredRawMaterialId = resin.Id, Specifications = "60x90cm, ocean pour technique, blue/teal tones" }
                    },
                    StatusHistory = new List<BoProductionRequestStatusHistory>
                    {
                        new() { Status = BoProductionRequestStatus.Submitted,   ChangedByUserId = craftsBoId, Notes = "Request submitted.",                      CreatedAt = DateTime.UtcNow.AddDays(-25) },
                        new() { Status = BoProductionRequestStatus.UnderReview, ChangedByUserId = AdminId,    Notes = "Reviewing feasibility.",                  CreatedAt = DateTime.UtcNow.AddDays(-23) },
                        new() { Status = BoProductionRequestStatus.Rejected,    ChangedByUserId = AdminId,    Notes = "Large format not currently supported.",   CreatedAt = DateTime.UtcNow.AddDays(-21) }
                    }
                },
 
                // 7 — Cancelled by BO
                new()
                {
                    BusinessOwnerId         = craftsBoId,
                    Title                   = "Macramé Wall Hangings — Summer Market",
                    Notes                   = "30 large macramé wall hangings for upcoming market stall.",
                    Status                  = BoProductionRequestStatus.Cancelled,
                    QuotedPrice             = 4500.00m,
                    AdminNotes              = "Quote sent — awaiting confirmation.",
                    EstimatedCompletionDate = DateTime.UtcNow.AddDays(-5),
                    CompletedAt             = null,
                    CreatedAt               = DateTime.UtcNow.AddDays(-20),
                    Items = new List<BoProductionRequestItem>
                    {
                        new() { ProductType = "Macramé Wall Hanging", Quantity = 30, PreferredRawMaterialId = macrame.Id, Specifications = "80cm wide x 120cm long, natural cotton cord" }
                    },
                    StatusHistory = new List<BoProductionRequestStatusHistory>
                    {
                        new() { Status = BoProductionRequestStatus.Submitted,   ChangedByUserId = craftsBoId, Notes = "Request submitted.",                          CreatedAt = DateTime.UtcNow.AddDays(-20) },
                        new() { Status = BoProductionRequestStatus.UnderReview, ChangedByUserId = AdminId,    Notes = "Under review.",                               CreatedAt = DateTime.UtcNow.AddDays(-19) },
                        new() { Status = BoProductionRequestStatus.Quoted,      ChangedByUserId = AdminId,    Notes = "Quote: 4,500 EGP.",                           CreatedAt = DateTime.UtcNow.AddDays(-17) },
                        new() { Status = BoProductionRequestStatus.Cancelled,   ChangedByUserId = craftsBoId, Notes = "Cancelled — market date moved, no longer needed.", CreatedAt = DateTime.UtcNow.AddDays(-15) }
                    }
                },
 
                // 8 — UnderReview
                new()
                {
                    BusinessOwnerId         = craftsBoId,
                    Title                   = "Resin Jewellery Set — Rings & Pendants",
                    Notes                   = "200 rings and 150 pendants. Dried flower inclusions preferred.",
                    Status                  = BoProductionRequestStatus.UnderReview,
                    QuotedPrice             = null,
                    AdminNotes              = null,
                    EstimatedCompletionDate = null,
                    CompletedAt             = null,
                    CreatedAt               = DateTime.UtcNow.AddDays(-4),
                    Items = new List<BoProductionRequestItem>
                    {
                        new() { ProductType = "Resin Ring",    Quantity = 200, PreferredRawMaterialId = resin.Id, Specifications = "Adjustable size, dried flowers encased" },
                        new() { ProductType = "Resin Pendant", Quantity = 150, PreferredRawMaterialId = resin.Id, Specifications = "Oval shape, 3cm, gold bail, dried flowers" }
                    },
                    StatusHistory = new List<BoProductionRequestStatusHistory>
                    {
                        new() { Status = BoProductionRequestStatus.Submitted,   ChangedByUserId = craftsBoId, Notes = "Request submitted.", CreatedAt = DateTime.UtcNow.AddDays(-4) },
                        new() { Status = BoProductionRequestStatus.UnderReview, ChangedByUserId = AdminId,    Notes = "Under review.",      CreatedAt = DateTime.UtcNow.AddDays(-3) }
                    }
                },
 
                // ══════════════════════════════════════════════════
                // BEAUTY BO — Salma Tarek
                // ══════════════════════════════════════════════════
 
                // 9 — Completed
                new()
                {
                    BusinessOwnerId         = beautyBoId,
                    Title                   = "Shea Body Butter — 300 Jars",
                    Notes                   = "Whipped shea body butter in 100ml glass jars. Lavender and unscented variants.",
                    Status                  = BoProductionRequestStatus.Completed,
                    QuotedPrice             = 8400.00m,
                    AdminNotes              = "All 300 jars filled, labelled, and sealed. Ready for collection.",
                    EstimatedCompletionDate = DateTime.UtcNow.AddDays(-30),
                    CompletedAt             = DateTime.UtcNow.AddDays(-28),
                    CreatedAt               = DateTime.UtcNow.AddDays(-75),
                    Items = new List<BoProductionRequestItem>
                    {
                        new() { ProductType = "Whipped Body Butter", Quantity = 300, PreferredRawMaterialId = sheaButter.Id, Specifications = "200 lavender scented, 100 unscented. 100ml glass jars." }
                    },
                    StatusHistory = new List<BoProductionRequestStatusHistory>
                    {
                        new() { Status = BoProductionRequestStatus.Submitted,    ChangedByUserId = beautyBoId, Notes = "Request submitted.",            CreatedAt = DateTime.UtcNow.AddDays(-75) },
                        new() { Status = BoProductionRequestStatus.UnderReview,  ChangedByUserId = AdminId,    Notes = "Under review.",                 CreatedAt = DateTime.UtcNow.AddDays(-73) },
                        new() { Status = BoProductionRequestStatus.Quoted,       ChangedByUserId = AdminId,    Notes = "Quote: 8,400 EGP.",             CreatedAt = DateTime.UtcNow.AddDays(-71) },
                        new() { Status = BoProductionRequestStatus.Confirmed,    ChangedByUserId = beautyBoId, Notes = "Confirmed.",                    CreatedAt = DateTime.UtcNow.AddDays(-70) },
                        new() { Status = BoProductionRequestStatus.InProduction, ChangedByUserId = AdminId,    Notes = "Production started.",           CreatedAt = DateTime.UtcNow.AddDays(-69) },
                        new() { Status = BoProductionRequestStatus.Completed,    ChangedByUserId = AdminId,    Notes = "300 jars ready for collection.", CreatedAt = DateTime.UtcNow.AddDays(-28) }
                    }
                },
 
                // 10 — Confirmed (quote accepted, waiting to start)
                new()
                {
                    BusinessOwnerId         = beautyBoId,
                    Title                   = "Glycerin Soap Bars — Rose & Honey Collection",
                    Notes                   = "400 soap bars in 4 variants: rose, honey & oat, charcoal, and plain. 90g each.",
                    Status                  = BoProductionRequestStatus.Confirmed,
                    QuotedPrice             = 6200.00m,
                    AdminNotes              = "Quote accepted. Production will begin within 2 business days.",
                    EstimatedCompletionDate = DateTime.UtcNow.AddDays(18),
                    CompletedAt             = null,
                    CreatedAt               = DateTime.UtcNow.AddDays(-12),
                    Items = new List<BoProductionRequestItem>
                    {
                        new() { ProductType = "Soap Bar", Quantity = 400, PreferredRawMaterialId = soapBase.Id, Specifications = "4 variants x 100 units. Rose, honey & oat, charcoal, plain. 90g each." }
                    },
                    StatusHistory = new List<BoProductionRequestStatusHistory>
                    {
                        new() { Status = BoProductionRequestStatus.Submitted,   ChangedByUserId = beautyBoId, Notes = "Request submitted.",       CreatedAt = DateTime.UtcNow.AddDays(-12) },
                        new() { Status = BoProductionRequestStatus.UnderReview, ChangedByUserId = AdminId,    Notes = "Under review.",            CreatedAt = DateTime.UtcNow.AddDays(-10) },
                        new() { Status = BoProductionRequestStatus.Quoted,      ChangedByUserId = AdminId,    Notes = "Quote: 6,200 EGP.",        CreatedAt = DateTime.UtcNow.AddDays(-8)  },
                        new() { Status = BoProductionRequestStatus.Confirmed,   ChangedByUserId = beautyBoId, Notes = "Quote confirmed by BO.",   CreatedAt = DateTime.UtcNow.AddDays(-7)  }
                    }
                },
 
                // 11 — Submitted (fresh, nothing done yet)
                new()
                {
                    BusinessOwnerId         = beautyBoId,
                    Title                   = "Argan Oil Serum — 150 Bottles",
                    Notes                   = "150 bottles of argan + vitamin E serum in 30ml amber dropper bottles. Custom label included.",
                    Status                  = BoProductionRequestStatus.Submitted,
                    QuotedPrice             = null,
                    AdminNotes              = null,
                    EstimatedCompletionDate = null,
                    CompletedAt             = null,
                    CreatedAt               = DateTime.UtcNow.AddHours(-5),
                    Items = new List<BoProductionRequestItem>
                    {
                        new() { ProductType = "Argan Oil Face Serum", Quantity = 150, PreferredRawMaterialId = null, Specifications = "30ml amber dropper bottle. Argan + Vitamin E blend. Include custom label." }
                    },
                    StatusHistory = new List<BoProductionRequestStatusHistory>
                    {
                        new() { Status = BoProductionRequestStatus.Submitted, ChangedByUserId = beautyBoId, Notes = "Request submitted.", CreatedAt = DateTime.UtcNow.AddHours(-5) }
                    }
                }
            };

            context.Set<BoProductionRequest>().AddRange(requests);
            await context.SaveChangesAsync();
        }
    }
}