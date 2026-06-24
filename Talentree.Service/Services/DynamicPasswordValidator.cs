using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Talentree.Core;
using Talentree.Core.Entities.Identity;

namespace Talentree.Service.Services
{
    public class DynamicPasswordValidator : IPasswordValidator<AppUser>
    {
        private readonly IServiceProvider _serviceProvider;

        public DynamicPasswordValidator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user, string? password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Password cannot be empty." });
            }

            await using var scope = _serviceProvider.CreateAsyncScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // Retrieve database settings (Id = 1)
            var settings = await unitOfWork.Repository<SecuritySettings>().GetByIdAsync(1);
            if (settings == null)
            {
                settings = new SecuritySettings(); // safe fallback defaults
            }

            var errors = new List<IdentityError>();

            // Only apply rules to admins as per the prompt: "Applied globally to all admin accounts"
            var roles = await manager.GetRolesAsync(user);
            var isAdmin = roles.Any(r => r == "SuperAdmin" || r == "Admin" || r == "SupportStaff" || r == "ContentManager");

            if (isAdmin)
            {
                if (password.Length < settings.PasswordRequiredLength)
                {
                    errors.Add(new IdentityError
                    {
                        Code = "PasswordTooShort",
                        Description = $"Password must be at least {settings.PasswordRequiredLength} characters long."
                    });
                }
                if (settings.PasswordRequireDigit && !password.Any(char.IsDigit))
                {
                    errors.Add(new IdentityError
                    {
                        Code = "PasswordRequiresDigit",
                        Description = "Password must contain at least one digit."
                    });
                }
                if (settings.PasswordRequireLowercase && !password.Any(char.IsLower))
                {
                    errors.Add(new IdentityError
                    {
                        Code = "PasswordRequiresLower",
                        Description = "Password must contain at least one lowercase character."
                    });
                }
                if (settings.PasswordRequireUppercase && !password.Any(char.IsUpper))
                {
                    errors.Add(new IdentityError
                    {
                        Code = "PasswordRequiresUpper",
                        Description = "Password must contain at least one uppercase character."
                    });
                }
                if (settings.PasswordRequireNonAlphanumeric && !password.Any(c => !char.IsLetterOrDigit(c)))
                {
                    errors.Add(new IdentityError
                    {
                        Code = "PasswordRequiresNonAlphanumeric",
                        Description = "Password must contain at least one special character."
                    });
                }
            }
            else
            {
                // Basic default validation for regular users
                if (password.Length < 8)
                {
                    errors.Add(new IdentityError
                    {
                        Code = "PasswordTooShort",
                        Description = "Password must be at least 8 characters long."
                    });
                }
            }

            return errors.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
        }
    }
}
