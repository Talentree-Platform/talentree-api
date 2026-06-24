using FluentValidation;
using Talentree.Service.DTOs.Admin;

namespace Talentree.Service.Validators.Admin
{
    public class UpdateSecuritySettingsDtoValidator : AbstractValidator<UpdateSecuritySettingsDto>
    {
        public UpdateSecuritySettingsDtoValidator()
        {
            RuleFor(x => x.PasswordRequiredLength)
                .InclusiveBetween(6, 32)
                .WithMessage("Password required length must be between 6 and 32 characters.");

            RuleFor(x => x.MaxFailedAccessAttempts)
                .InclusiveBetween(3, 20)
                .WithMessage("Max failed access attempts must be between 3 and 20.");

            RuleFor(x => x.LockoutDurationInMinutes)
                .InclusiveBetween(1, 1440)
                .WithMessage("Lockout duration must be between 1 and 1440 minutes.");

            RuleFor(x => x.SessionTimeoutInMinutes)
                .InclusiveBetween(5, 1440)
                .WithMessage("Session timeout must be between 5 and 1440 minutes.");

            RuleFor(x => x.IpWhitelist)
                .MaximumLength(2000)
                .WithMessage("IP whitelist string length cannot exceed 2000 characters.");

            // Start time and End time must both be null, or both must be set
            RuleFor(x => x)
                .Must(x => (x.AllowedLoginStartTime == null && x.AllowedLoginEndTime == null) ||
                           (x.AllowedLoginStartTime != null && x.AllowedLoginEndTime != null))
                .WithMessage("Both AllowedLoginStartTime and AllowedLoginEndTime must be set or both must be null.");
        }
    }
}
