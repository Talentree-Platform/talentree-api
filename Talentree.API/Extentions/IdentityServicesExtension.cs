using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Talentree.Core.Entities.Identity;
using Talentree.Repository.Identity;

namespace Talentree.API.Extensions
{
    public static class IdentityServicesExtension
    {
        public static IServiceCollection AddIdentityServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // 1️⃣ Configure Identity
            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;

                // User settings
                options.User.RequireUniqueEmail = true;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddEntityFrameworkStores<AppIdentityDbContext>()
            .AddDefaultTokenProviders();

            // 2️⃣ Configure JWT Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = configuration["JWT:Issuer"],

                    ValidateAudience = true,
                    ValidAudience = configuration["JWT:Audience"],

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["JWT:Key"]))
                };
            });

            return services;
        }
    }
}