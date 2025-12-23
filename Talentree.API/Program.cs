
using Microsoft.EntityFrameworkCore;
using Talentree.API.Extensions;
using Talentree.API.Extentions;
using Talentree.Core;
using Talentree.Repository.Data;
using Talentree.Repository.Identity;

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


          
            // ===============================
            // Register DbContext with SQL Server
            // ===============================
            builder.Services.AddDbContext<TalentreeDbContext>(options =>
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

                // Enable extra logging and detailed errors only during development
                // Do NOT enable this in Production — it may expose sensitive data
                if (builder.Environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging(); // Logs actual SQL parameter values (debugging only)
                    options.EnableDetailedErrors();       // Provides detailed EF Core error messages
                }
            });

            builder.Services.AddApplicationServices();


            // Identity DbContext
            builder.Services.AddDbContext<AppIdentityDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            //Identity Services
            builder.Services.AddIdentityServices(builder.Configuration);







            var app = builder.Build();

            // ===============================
            // Database Migration (Development Only)
            // ===============================
            // Only run auto-migration in Development environment
            // In Production, migrations should be applied manually or via CI/CD
            if (app.Environment.IsDevelopment())
            {
                await app.MigrateDatabaseAsync();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication(); 
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
