using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Talentree.Core.Entities.Identity;

namespace Talentree.Repository.Identity
{
    public static class AppIdentityDbContextSeed
    {
        public static async Task SeedAsync(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger logger)
        {
            try
            {
                await SeedRolesAsync(roleManager, logger);
                await SeedAdminUserAsync(userManager, logger);
                await SeedTestUsersAsync(userManager, logger);

                logger.LogInformation("✅ Identity seeding completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ An error occurred during identity seeding");
                throw;
            }
        }

        private static async Task SeedRolesAsync(
            RoleManager<IdentityRole> roleManager,
            ILogger logger)
        {
            logger.LogInformation("Starting roles seeding...");

            string[] roles = { "Admin", "BusinessOwner", "Customer" };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(roleName));

                    if (result.Succeeded)
                    {
                        logger.LogInformation($"✅ Role '{roleName}' created successfully");
                    }
                    else
                    {
                        logger.LogWarning($"⚠️ Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    logger.LogInformation($"⏭️ Role '{roleName}' already exists, skipping");
                }
            }
        }

        private static async Task SeedAdminUserAsync(
            UserManager<AppUser> userManager,
            ILogger logger)
        {
            logger.LogInformation("Starting admin user seeding...");

            const string adminEmail = "admin@talentree.com";
            const string adminPassword = "Admin@123456";

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

            if (existingAdmin == null)
            {
                var adminUser = new AppUser
                {
                    DisplayName = "System Administrator",
                    Email = adminEmail,
                    UserName = adminEmail,
                    PhoneNumber = "01000000000",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    logger.LogInformation($"✅ Admin user created: {adminEmail}");
                    logger.LogInformation($"   Password: {adminPassword}");
                }
                else
                {
                    logger.LogWarning($"⚠️ Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                logger.LogInformation($"⏭️ Admin user already exists: {adminEmail}");
            }
        }

        private static async Task SeedTestUsersAsync(
            UserManager<AppUser> userManager,
            ILogger logger)
        {
            logger.LogInformation("Starting test users seeding...");

            var testUsers = new[]
            {
                new
                {
                    Email = "business@talentree.com",
                    DisplayName = "Test Business Owner",
                    Role = "BusinessOwner",
                    Phone = "01111111111"
                },
                new
                {
                    Email = "customer@talentree.com",
                    DisplayName = "Test Customer",
                    Role = "Customer",
                    Phone = "01222222222"
                }
            };

            foreach (var userData in testUsers)
            {
                var existingUser = await userManager.FindByEmailAsync(userData.Email);

                if (existingUser == null)
                {
                    var user = new AppUser
                    {
                        DisplayName = userData.DisplayName,
                        Email = userData.Email,
                        UserName = userData.Email,
                        PhoneNumber = userData.Phone,
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    const string testPassword = "Test@123456";
                    var result = await userManager.CreateAsync(user, testPassword);

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, userData.Role);
                        logger.LogInformation($"✅ Test user created: {userData.Email} (Role: {userData.Role})");
                    }
                    else
                    {
                        logger.LogWarning($"⚠️ Failed to create test user {userData.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    logger.LogInformation($"⏭️ Test user already exists: {userData.Email}");
                }
            }
        }
    }
}