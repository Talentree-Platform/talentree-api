using Microsoft.AspNetCore.Identity;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.DataSeed
{
    public static class DbInitializer
    {
        public static async Task SeedAllAsync(
            TalentreeDbContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<AppUser> userManager)
        {
            // 1 — Roles must exist before any user is created
            await SeedRolesAsync(roleManager);

            // 2 — BO accounts (AppUser + BusinessOwnerProfile)
            //     Must run before MaterialOrderSeed so user IDs exist in AspNetUsers
            await BusinessOwnerSeed.SeedAsync(context, userManager);

            // 3 — Material orders — references BO IDs and raw material IDs
            await MaterialOrderSeed.SeedAsync(context);

            // 4 — Production requests — references BO IDs and raw material IDs
            await ProductionRequestSeed.SeedAsync(context);
        }

        // ── Seed Roles ────────────────────────────────────────────
        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = ["Admin", "BusinessOwner"];

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}