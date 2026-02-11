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

            builder.Services.ValidationServices();

            builder.Services.AddControllers();

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
                var interceptor = serviceProvider.GetRequiredService<AuditInterceptor>();

                var connectionString = builder.Environment.IsDevelopment()
                    ? builder.Configuration.GetConnectionString("DefaultConnection")  // Local
                    : builder.Configuration.GetConnectionString("DeploymentConnection");  // Remote / Deploy

                //var connectionString = builder.Configuration.GetConnectionString("DeploymentConnection");

                options.UseSqlServer(
                    connectionString,
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(TalentreeDbContext).Assembly.FullName);

                        // Enable retry on failure for transient faults in production
                        if (!builder.Environment.IsDevelopment())
                        {
                            sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
                        }
                    });

                options.AddInterceptors(interceptor);

                // Enable sensitive logging only in  Development
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
                .AddAuthentication(options =>
                {
                    // define the default authentication scheme as JWT Bearer not Cookies identification
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
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

            builder.Services.AddAuthorization();

            // ===============================
            // AutoMapper
            // ===============================
            builder.Services.AddAutoMapper(typeof(MappingProfile));

            //CORs 
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });



            var app = builder.Build();

            //Middleware  Pipeline
            app.UseCors("AllowAll");

            // ===============================
            // Migrate DB
            // ===============================
            if (app.Environment.IsDevelopment())
            {
                await app.MigrateDatabaseAsync();
            }


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
