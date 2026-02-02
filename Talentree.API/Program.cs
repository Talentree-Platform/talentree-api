using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Talentree.API.Extensions;
using Talentree.API.Extentions;
using Talentree.Core;
using Talentree.Repository.Data;
using Talentree.Repository.Data.Interceptors;
using Talentree.Service.Mapping;

namespace Talentree.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ===============================
            // MVC + Validation
            // ===============================
            builder.Services
                .AddControllers()
                .AddFluentValidation(); // لو مستخدمة FluentValidation extension

            builder.Services.ValidationServices();

            // ===============================
            // Swagger
            // ===============================
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // ===============================
            // HttpContext + Interceptors
            // ===============================
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<AuditInterceptor>();

            // ===============================
            // DbContext
            // ===============================
            builder.Services.AddDbContext<TalentreeDbContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(TalentreeDbContext).Assembly.FullName);
                        sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
                    });

                options.AddInterceptors(serviceProvider.GetRequiredService<AuditInterceptor>());

                if (builder.Environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            });

            // ===============================
            // Identity
            // ===============================
            builder.Services
                .AddIdentity<AppUser, IdentityRole>(options =>
                {
                    options.Password.RequiredLength = 8;
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<TalentreeDbContext>()
                .AddDefaultTokenProviders();

            // ===============================
            // Application Services (DI)
            // ===============================
            builder.Services.AddApplicationServices();

            // ===============================
            // Authentication (JWT)
            // ===============================
            var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]);

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

            builder.Services.AddAuthorization();

            // ===============================
            // AutoMapper
            // ===============================
            builder.Services.AddAutoMapper(typeof(MappingProfile));

            var app = builder.Build();

            // ===============================
            // Migrate DB
            // ===============================
            await app.MigrateDatabaseAsync();

            // ===============================
            // Middleware pipeline
            // ===============================
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseGlobalExceptionHandling();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
