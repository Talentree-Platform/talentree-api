using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Talentree.API.Extensions;
using Talentree.API.Extentions;
using Talentree.API.Hubs;
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

            // ===============================
            // MVC + Validation
            // ===============================
            builder.Services.ValidationServices();
            builder.Services.AddControllers();

            // ===============================
            // Swagger + JWT Authorization
            // ===============================
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Paste only the JWT token here (do NOT include 'Bearer'). Swagger will add it automatically."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {} // No scopes required for JWT Bearer
                    }
                });
            });

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
                    ? builder.Configuration.GetConnectionString("DefaultConnection")
                    : builder.Configuration.GetConnectionString("DeploymentConnection");

                options.UseSqlServer(
                    connectionString,
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(TalentreeDbContext).Assembly.FullName);
                        if (!builder.Environment.IsDevelopment())
                            sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
                    });

                options.AddInterceptors(interceptor);

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
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireDigit = true;
                    options.Password.RequireNonAlphanumeric = true;
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
            var key = Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"]);

            builder.Services
                .AddAuthentication(options =>
                {
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
                        ValidIssuer = builder.Configuration["JWT:Issuer"],
                        ValidAudience = builder.Configuration["JWT:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ClockSkew = TimeSpan.Zero
                    };


                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            // Get token from query string (SignalR sends it this way)
                            var accessToken = context.Request.Query["access_token"];

                            // Check if request is for SignalR hub
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                path.StartsWithSegments("/hubs"))
                            {
                                // Set token for authentication
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddAuthorization();

            // ===============================
            // AutoMapper
            // ===============================
            builder.Services.AddAutoMapper(typeof(MappingProfile));


            // ═══════════════════════════════════════════════════════════
            // ADD SIGNALR
            // ═══════════════════════════════════════════════════════════

            builder.Services.AddSignalR(options =>
            {
                // Enable detailed errors in development
                options.EnableDetailedErrors = builder.Environment.IsDevelopment();

                // Keep-alive interval (ping client every 15 seconds)
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);

                // Client timeout (disconnect if no response for 30 seconds)
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);

                // Max message size (10MB)
                options.MaximumReceiveMessageSize = 10 * 1024 * 1024;
            });

            // ===============================
            // CORS
            // ===============================

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.WithOrigins(
                            "http://localhost:5500",
                            "http://127.0.0.1:5500",
                            "http://localhost:4200"
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
                });
            });

            // SignalR
            builder.Services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = builder.Environment.IsDevelopment();
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
            });
            var app = builder.Build();
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<TalentreeDbContext>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

                await DbInitializer.SeedAllAsync(context, roleManager, userManager);
            }
            // ===============================
            // Middleware pipeline
            // ===============================


            // DB Migration
            await app.MigrateDatabaseAsync();

            // Swagger middleware
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseCors("AllowAll");
            app.UseGlobalExceptionHandling();

            app.UseAuthentication(); // MUST be before Authorization
            app.UseAuthorization();

            // ═══════════════════════════════════════════════════════════
            // MAP SIGNALR HUB ENDPOINT
            // ═══════════════════════════════════════════════════════════

            app.MapHub<NotificationHub>("/hubs/notification");

            app.UseStaticFiles();

            app.MapControllers();

            app.Run();
        }
    }
}
