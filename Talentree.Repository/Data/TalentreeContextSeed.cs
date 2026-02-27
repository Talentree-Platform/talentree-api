// Talentree.Repository/Identity/AppIdentityDbContextSeed.cs

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;
using Talentree.Repository.Data;

namespace Talentree.Repository.Data
{
    /// <summary>
    /// Seeds initial identity data (roles and users)
    /// </summary>
    public static class TalentreeContextSeed
    {
        public static async Task SeedAsync(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            TalentreeDbContext context,
            ILogger logger)
        {
            try
            {
                // ═══════════════════════════════════════════════════════════
                // STEP 1: Seed Roles
                // ═══════════════════════════════════════════════════════════
                await SeedRolesAsync(roleManager, logger);

                // ═══════════════════════════════════════════════════════════
                // STEP 2: Seed Admin User
                // ═══════════════════════════════════════════════════════════
                await SeedAdminUserAsync(userManager, logger);

                // ═══════════════════════════════════════════════════════════
                // STEP 3: Seed Business Owners
                // ═══════════════════════════════════════════════════════════
                await SeedBusinessOwnersAsync(userManager, context, logger);

                // ═══════════════════════════════════════════════════════════
                // STEP 4: Seed Customers
                // ═══════════════════════════════════════════════════════════
                await SeedCustomersAsync(userManager, logger);

                logger.LogInformation("✅ Identity seeding completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ An error occurred during identity seeding");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // SEED ROLES
        // ═══════════════════════════════════════════════════════════
        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            var roles = new[] { "Admin", "BusinessOwner", "Customer" };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var role = new IdentityRole(roleName);
                    await roleManager.CreateAsync(role);
                    logger.LogInformation($"✅ Role created: {roleName}");
                }
            }
        }

        // ═══════════════════════════════════════════════════════════
        // SEED ADMIN USER
        // ═══════════════════════════════════════════════════════════
        private static async Task SeedAdminUserAsync(UserManager<AppUser> userManager, ILogger logger)
        {
            var adminEmail = "admin@talentree.com";

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                var admin = new AppUser
                {
                    DisplayName = "System Administrator",
                    Email = adminEmail,
                    UserName = adminEmail,
                    PhoneNumber = "01000000000",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(admin, "Admin@123456");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                    logger.LogInformation($" Admin user created: {adminEmail}");
                }
                else
                {
                    logger.LogError($" Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        // ═══════════════════════════════════════════════════════════
        // SEED BUSINESS OWNERS
        // ═══════════════════════════════════════════════════════════
        private static async Task SeedBusinessOwnersAsync(
            UserManager<AppUser> userManager,
            TalentreeDbContext context,
            ILogger logger)
        {
            var businessOwners = new BusinessOwnerSeedData[]
            {
                new BusinessOwnerSeedData
                {
                    User = new AppUser
                    {
                        DisplayName = "Ahmed Hassan",
                        Email = "ahmed.hassan@example.com",
                        UserName = "ahmed.hassan@example.com",
                        PhoneNumber = "01234567890",
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-10)
                    },
                    Password = "Test@123456",
                    Profile = new BusinessOwnerProfileSeedData
                    {
                        BusinessName = "Tech Galaxy",
                        BusinessCategory = "Electronics",
                        BusinessDescription = "Leading electronics retailer with latest gadgets and devices",
                        BusinessAddress = "123 Tech Street, Cairo, Egypt",
                        TaxId = "123456789",
                        FacebookLink = "https://facebook.com/techgalaxy",
                        InstagramLink = "https://instagram.com/techgalaxy",
                        WebsiteLink = "https://techgalaxy.com",
                        Status = ApprovalStatus.Approved,
                        ApprovedAt = DateTime.UtcNow.AddDays(-9),
                        ApprovedBy = "admin@talentree.com"
                    }
                },
                new BusinessOwnerSeedData
                {
                    User = new AppUser
                    {
                        DisplayName = "Fatma Mohamed",
                        Email = "fatma.mohamed@example.com",
                        UserName = "fatma.mohamed@example.com",
                        PhoneNumber = "01123456789",
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-5)
                    },
                    Password = "Test@123456",
                    Profile = new BusinessOwnerProfileSeedData
                    {
                        BusinessName = "Fashion House",
                        BusinessCategory = "Fashion",
                        BusinessDescription = "Trendy fashion boutique for modern women",
                        BusinessAddress = "456 Fashion Ave, Alexandria, Egypt",
                        TaxId = "987654321",
                        FacebookLink = "https://facebook.com/fashionhouse",
                        InstagramLink = "https://instagram.com/fashionhouse",
                        WebsiteLink = (string?)null,
                        Status = ApprovalStatus.Pending,
                        ApprovedAt = (DateTime?)null,
                        ApprovedBy = (string?)null
                    }
                },
                new BusinessOwnerSeedData
                {
                    User = new AppUser
                    {
                        DisplayName = "Mahmoud Ali",
                        Email = "mahmoud.ali@example.com",
                        UserName = "mahmoud.ali@example.com",
                        PhoneNumber = "01098765432",
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-15)
                    },
                    Password = "Test@123456",
                    Profile = new BusinessOwnerProfileSeedData
                    {
                        BusinessName = "Fresh Mart",
                        BusinessCategory = "Food",
                        BusinessDescription = "Organic fresh produce and groceries",
                        BusinessAddress = "789 Market St, Giza, Egypt",
                        TaxId = "555666777",
                        FacebookLink = (string?)null,
                        InstagramLink = (string?)null,
                        WebsiteLink = (string?)null,
                        Status = ApprovalStatus.Rejected,
                        ApprovedAt = DateTime.UtcNow.AddDays(-14),
                        ApprovedBy = "admin@talentree.com"
                    }
                },
                new BusinessOwnerSeedData
                {
                    User = new AppUser
                    {
                        DisplayName = "Sara Ibrahim",
                        Email = "sara.ibrahim@example.com",
                        UserName = "sara.ibrahim@example.com",
                        PhoneNumber = "01187654321",
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-3)
                    },
                    Password = "Test@123456",
                    Profile = new BusinessOwnerProfileSeedData
                    {
                        BusinessName = "Beauty Corner",
                        BusinessCategory = "Beauty",
                        BusinessDescription = "Premium beauty and cosmetics store",
                        BusinessAddress = "321 Beauty Lane, Cairo, Egypt",
                        TaxId = "111222333",
                        FacebookLink = "https://facebook.com/beautycorner",
                        InstagramLink = "https://instagram.com/beautycorner",
                        WebsiteLink = "https://beautycorner.com",
                        Status = ApprovalStatus.Approved,
                        ApprovedAt = DateTime.UtcNow.AddDays(-2),
                        ApprovedBy = "admin@talentree.com"
                    }
                },
                new BusinessOwnerSeedData
                {
                    User = new AppUser
                    {
                        DisplayName = "Omar Youssef",
                        Email = "omar.youssef@example.com",
                        UserName = "omar.youssef@example.com",
                        PhoneNumber = "01065432109",
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddHours(-6) // Recent application
                    },
                    Password = "Test@123456",
                    Profile = new BusinessOwnerProfileSeedData
                    {
                        BusinessName = "Sports Zone",
                        BusinessCategory = "Sports",
                        BusinessDescription = "Athletic gear and sports equipment",
                        BusinessAddress = "654 Sports Blvd, Cairo, Egypt",
                        TaxId = "444555666",
                        FacebookLink = (string?)null,
                        InstagramLink = "https://instagram.com/sportszone",
                        WebsiteLink = (string?)null,
                        Status = ApprovalStatus.Pending,
                        ApprovedAt = (DateTime?)null,
                        ApprovedBy = (string?)null
                    }
                }
            };

            foreach (var bo in businessOwners)
            {
                var existingUser = await userManager.FindByEmailAsync(bo.User.Email!);
                if (existingUser == null)
                {
                    var result = await userManager.CreateAsync(bo.User, bo.Password);

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(bo.User, "BusinessOwner");

                        var profile = new BusinessOwnerProfile
                        {
                            UserId = bo.User.Id,
                            BusinessName = bo.Profile.BusinessName,
                            BusinessCategory = bo.Profile.BusinessCategory,
                            BusinessDescription = bo.Profile.BusinessDescription,
                            BusinessAddress = bo.Profile.BusinessAddress,
                            TaxId = bo.Profile.TaxId,
                            FacebookLink = bo.Profile.FacebookLink,
                            InstagramLink = bo.Profile.InstagramLink,
                            WebsiteLink = bo.Profile.WebsiteLink,
                            Status = bo.Profile.Status,
                            ApprovedAt = bo.Profile.ApprovedAt,
                            ApprovedBy = bo.Profile.ApprovedBy,
                            AutoApprovalDeadline = bo.Profile.Status == ApprovalStatus.Pending
                                ? DateTime.UtcNow.AddHours(12)
                                : null,
                            CreatedAt = bo.User.CreatedAt
                        };

                        // Add rejection reason for rejected profiles
                        if (bo.Profile.Status == ApprovalStatus.Rejected)
                        {
                            profile.RejectionReason = "Incomplete business documentation provided";
                        }

                        context.BusinessOwnerProfiles.Add(profile);

                        logger.LogInformation($"✅ Business owner created: {bo.User.Email} ({bo.Profile.Status})");
                    }
                }
            }

            await context.SaveChangesAsync();
        }

        // ═══════════════════════════════════════════════════════════
        // SEED CUSTOMERS
        // ═══════════════════════════════════════════════════════════
        private static async Task SeedCustomersAsync(UserManager<AppUser> userManager, ILogger logger)
        {
            var customers = new[]
            {
                new
                {
                    DisplayName = "Mona Ahmed",
                    Email = "mona.ahmed@example.com",
                    PhoneNumber = "01234567111",
                    Password = "Test@123456"
                },
                new
                {
                    DisplayName = "Khaled Mahmoud",
                    Email = "khaled.mahmoud@example.com",
                    PhoneNumber = "01234567222",
                    Password = "Test@123456"
                },
                new
                {
                    DisplayName = "Nour Hassan",
                    Email = "nour.hassan@example.com",
                    PhoneNumber = "01234567333",
                    Password = "Test@123456"
                },
                new
                {
                    DisplayName = "Yasser Ali",
                    Email = "yasser.ali@example.com",
                    PhoneNumber = "01234567444",
                    Password = "Test@123456"
                },
                new
                {
                    DisplayName = "Laila Mohamed",
                    Email = "laila.mohamed@example.com",
                    PhoneNumber = "01234567555",
                    Password = "Test@123456"
                }
            };

            foreach (var customer in customers)
            {
                var existingUser = await userManager.FindByEmailAsync(customer.Email);
                if (existingUser == null)
                {
                    var user = new AppUser
                    {
                        DisplayName = customer.DisplayName,
                        Email = customer.Email,
                        UserName = customer.Email,
                        PhoneNumber = customer.PhoneNumber,
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 30))
                    };

                    var result = await userManager.CreateAsync(user, customer.Password);

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Customer");
                        logger.LogInformation($"✅ Customer created: {customer.Email}");
                    }
                }
            }
        }


        private class BusinessOwnerSeedData
        {
            public AppUser User { get; set; } = default!;
            public string Password { get; set; } = default!;
            public BusinessOwnerProfileSeedData Profile { get; set; } = default!;
        }

        private class BusinessOwnerProfileSeedData
        {
            public string BusinessName { get; set; } = default!;
            public string BusinessCategory { get; set; } = default!;
            public string BusinessDescription { get; set; } = default!;
            public string BusinessAddress { get; set; } = default!;
            public string TaxId { get; set; } = default!;
            public string? FacebookLink { get; set; }
            public string? InstagramLink { get; set; }
            public string? WebsiteLink { get; set; }
            public ApprovalStatus Status { get; set; }
            public DateTime? ApprovedAt { get; set; }
            public string? ApprovedBy { get; set; }
        }
    }
}