
using Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Talentree.API.Extensions;
using Talentree.API.Extentions;
using Talentree.Core;
using Talentree.Repository.Data;
using Talentree.Repository.Data.DataSeed;
using Talentree.Repository.Data.Interceptors;
using Talentree.Service.Mapping;

namespace Talentree.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // ⭐ Register HttpContextAccessor (needed for AuditInterceptor)
            builder.Services.AddHttpContextAccessor();

            // ⭐ Register AuditInterceptor
            builder.Services.AddScoped<AuditInterceptor>();

        
            // ===============================
            // Register DbContext with SQL Server
            // ===============================
            builder.Services.AddDbContext<TalentreeDbContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),

                    sqlOptions =>
                    {
                        // Specify where EF Core should look for migrations
                        sqlOptions.MigrationsAssembly(typeof(TalentreeDbContext).Assembly.FullName);

                        // Enable automatic retry logic for transient SQL errors
                        // Useful in cloud environments and during database restarts
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,                    // Number of retry attempts
                            maxRetryDelay: TimeSpan.FromSeconds(30), // Max delay between retries
                            errorNumbersToAdd: null);            // Use default SQL transient errors
                    });

                // Add the interceptor
                options.AddInterceptors(serviceProvider.GetRequiredService<AuditInterceptor>());

                // Enable extra logging and detailed errors only during development
                // Do NOT enable this in Production — it may expose sensitive data
                if (builder.Environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging(); // Logs actual SQL parameter values (debugging only)
                    options.EnableDetailedErrors();       // Provides detailed EF Core error messages
                }
            });

            //Identity Services
            builder.Services
                .AddIdentity<AppUser, IdentityRole>(options =>
                {
                    options.Password.RequiredLength = 8;
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<TalentreeDbContext>()
                .AddDefaultTokenProviders();


            //DI Services
            builder.Services.AddApplicationServices();
            //validators 
            builder.Services.ValidationServices();

        
            //JWT Authentication

            var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };
            });

            // Mapping Profiles
            builder.Services.AddAutoMapper(typeof(MappingProfile));






            var app = builder.Build();

            // ===============================
            // Database Migration 
            // ===============================
            await app.MigrateDatabaseAsync();
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();
            // Global Exception Handling Middleware
            app.UseGlobalExceptionHandling();

            app.UseAuthentication();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
