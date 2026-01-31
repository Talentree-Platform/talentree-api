// Talentree.Service/Validators/Auth/ResetPasswordDtoValidator.cs
using FluentValidation;
using Talentree.Service.DTOs.Auth;

namespace Talentree.Service.Validators.Auth
{
    public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.OtpCode)
                .NotEmpty().WithMessage("OTP code is required")
                .Length(6).WithMessage("OTP code must be 6 digits");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%?&])[A-Za-z\d@$!%?&]{8,}$")
                .WithMessage("Password must be at least 8 characters and contain uppercase, lowercase, number and special character");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Confirm password is required")
                .Equal(x => x.NewPassword).WithMessage("Passwords do not match");
        }
    }
}
