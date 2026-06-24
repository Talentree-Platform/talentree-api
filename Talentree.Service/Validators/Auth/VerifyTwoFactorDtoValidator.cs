using FluentValidation;
using Talentree.Service.DTOs.Auth;

namespace Talentree.Service.Validators.Auth
{
    public class VerifyTwoFactorDtoValidator : AbstractValidator<VerifyTwoFactorDto>
    {
        public VerifyTwoFactorDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.OtpCode)
                .NotEmpty().WithMessage("OTP code is required.")
                .Length(6).WithMessage("OTP code must be exactly 6 digits.")
                .Matches(@"^\d{6}$").WithMessage("OTP code must consist of numbers only.");
        }
    }
}
