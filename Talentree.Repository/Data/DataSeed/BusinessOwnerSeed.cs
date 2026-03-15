// ============================================================
// Talentree.Repository/Data/DataSeed/BusinessOwnerSeed.cs
// ============================================================
// Creates seeded BO accounts with fixed, known GUIDs so that
// MaterialOrderSeed can reference them without a DB lookup.
// Must run AFTER roles are seeded and BEFORE MaterialOrderSeed.
// ============================================================
using Microsoft.AspNetCore.Identity;
using Talentree.Core.Entities;
using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;

namespace Talentree.Repository.Data.DataSeed
{
    public static class BusinessOwnerSeed
    {
        // ── Fixed GUIDs — also used in MaterialOrderSeed ──────────
        // Copy these constants into MaterialOrderSeed to replace the placeholder strings.
        public const string FashionBoId = "11111111-1111-1111-1111-111111111101";
        public const string CraftsBoId = "22222222-2222-2222-2222-222222222202";
        public const string BeautyBoId = "33333333-3333-3333-3333-333333333303";

        public static async Task SeedAsync(
            TalentreeDbContext context,
            UserManager<AppUser> userManager)
        {
            // ── BO 1 — Fashion & Accessories ──────────────────────
            if (!userManager.Users.Any(u => u.Id == FashionBoId))
            {
                var fashionBo = new AppUser
                {
                    Id = FashionBoId,
                    DisplayName = "Nour El-Sayed",
                    UserName = "nour.elsayed@talentree.eg",
                    Email = "nour.elsayed@talentree.eg",
                    NormalizedEmail = "NOUR.ELSAYED@TALENTREE.EG",
                    NormalizedUserName = "NOUR.ELSAYED@TALENTREE.EG",
                    EmailConfirmed = true,
                    PhoneNumber = "+20-100-111-2233",
                    PhoneNumberConfirmed = true,
                    IsActive = true,
                    SecurityStamp = Guid.NewGuid().ToString()
                };
                await CreateUserAsync(userManager, fashionBo, "Seed@1234!", "BusinessOwner");
            }

            if (!context.Set<BusinessOwnerProfile>().Any(p => p.UserId == FashionBoId))
            {
                context.Set<BusinessOwnerProfile>().Add(new BusinessOwnerProfile
                {
                    UserId = FashionBoId,
                    BusinessName = "Nour Couture",
                    BusinessCategory = "Fashion & Accessories",
                    BusinessDescription = "Handmade women's fashion — modest wear, abayas and accessories crafted from premium Egyptian cotton and linen.",
                    BusinessAddress = "14 Al-Tahrir Square, Apt 3, Cairo",
                    FacebookLink = "https://facebook.com/nourcouture",
                    InstagramLink = "https://instagram.com/nourcouture.eg",
                    WebsiteLink = null,
                    TaxId = string.Empty,
                    Status = ApprovalStatus.Approved,
                    ApprovedAt = DateTime.UtcNow.AddDays(-30),
                    ApprovedBy = "seed",
                    RejectionReason = null,
                    AutoApprovalDeadline = null
                });
                await context.SaveChangesAsync();
            }

            // ── BO 2 — Handmade Crafts ────────────────────────────
            if (!userManager.Users.Any(u => u.Id == CraftsBoId))
            {
                var craftsBo = new AppUser
                {
                    Id = CraftsBoId,
                    DisplayName = "Karim Mansour",
                    UserName = "karim.mansour@talentree.eg",
                    Email = "karim.mansour@talentree.eg",
                    NormalizedEmail = "KARIM.MANSOUR@TALENTREE.EG",
                    NormalizedUserName = "KARIM.MANSOUR@TALENTREE.EG",
                    EmailConfirmed = true,
                    PhoneNumber = "+20-110-987-6543",
                    PhoneNumberConfirmed = true,
                    IsActive = true,
                    SecurityStamp = Guid.NewGuid().ToString()
                };
                await CreateUserAsync(userManager, craftsBo, "Seed@1234!", "BusinessOwner");
            }

            if (!context.Set<BusinessOwnerProfile>().Any(p => p.UserId == CraftsBoId))
            {
                context.Set<BusinessOwnerProfile>().Add(new BusinessOwnerProfile
                {
                    UserId = CraftsBoId,
                    BusinessName = "Karim Craft Studio",
                    BusinessCategory = "Handmade Crafts",
                    BusinessDescription = "Artisan candles, resin art, and macramé pieces. Every item is handmade in small batches in Alexandria.",
                    BusinessAddress = "7 Corniche El-Nil, Floor 2, Alexandria",
                    FacebookLink = "https://facebook.com/karimcraftstudio",
                    InstagramLink = "https://instagram.com/karimcraft.eg",
                    WebsiteLink = "https://karimcrafts.com",
                    TaxId = string.Empty,
                    Status = ApprovalStatus.Approved,
                    ApprovedAt = DateTime.UtcNow.AddDays(-45),
                    ApprovedBy = "seed",
                    RejectionReason = null,
                    AutoApprovalDeadline = null
                });
                await context.SaveChangesAsync();
            }

            // ── BO 3 — Natural & Beauty Products ──────────────────
            if (!userManager.Users.Any(u => u.Id == BeautyBoId))
            {
                var beautyBo = new AppUser
                {
                    Id = BeautyBoId,
                    DisplayName = "Salma Tarek",
                    UserName = "salma.tarek@talentree.eg",
                    Email = "salma.tarek@talentree.eg",
                    NormalizedEmail = "SALMA.TAREK@TALENTREE.EG",
                    NormalizedUserName = "SALMA.TAREK@TALENTREE.EG",
                    EmailConfirmed = true,
                    PhoneNumber = "+20-122-456-7890",
                    PhoneNumberConfirmed = true,
                    IsActive = true,
                    SecurityStamp = Guid.NewGuid().ToString()
                };
                await CreateUserAsync(userManager, beautyBo, "Seed@1234!", "BusinessOwner");
            }

            if (!context.Set<BusinessOwnerProfile>().Any(p => p.UserId == BeautyBoId))
            {
                context.Set<BusinessOwnerProfile>().Add(new BusinessOwnerProfile
                {
                    UserId = BeautyBoId,
                    BusinessName = "Salma Naturals",
                    BusinessCategory = "Natural & Beauty Products",
                    BusinessDescription = "Cold-process soaps, argan oil blends, and natural skincare made from organic Egyptian ingredients.",
                    BusinessAddress = "22 Hassan Allam Street, Giza",
                    FacebookLink = "https://facebook.com/salmanaturals",
                    InstagramLink = "https://instagram.com/salmanaturals.eg",
                    WebsiteLink = null,
                    TaxId = "TAX-EG-2024-7890",
                    Status = ApprovalStatus.Approved,
                    ApprovedAt = DateTime.UtcNow.AddDays(-60),
                    ApprovedBy = "seed",
                    RejectionReason = null,
                    AutoApprovalDeadline = null
                });
                await context.SaveChangesAsync();
            }
        }

        // ── Private helper ────────────────────────────────────────

        /// <summary>
        /// Creates a user with the given password and assigns a role.
        /// Throws if creation fails so seed errors surface immediately.
        /// </summary>
        private static async Task CreateUserAsync(
            UserManager<AppUser> userManager,
            AppUser user,
            string password,
            string role)
        {
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new Exception(
                    $"Failed to seed user '{user.Email}': " +
                    string.Join(", ", result.Errors.Select(e => e.Description)));

            await userManager.AddToRoleAsync(user, role);
        }
    }
}