using FluentValidation;
using Talentree.Service.DTOs.AccountSettings;

namespace Talentree.Service.Validators.AccountSettings
{
    public class UpdateProfileDtoValidator : AbstractValidator<UpdateProfileDto>
    {
        public UpdateProfileDtoValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.BusinessName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.BusinessDescription).NotEmpty().MaximumLength(2000);
            RuleFor(x => x.NewEmail).EmailAddress()
                .When(x => !string.IsNullOrEmpty(x.NewEmail));
            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?\d{7,15}$")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
            RuleFor(x => x.WebsiteLink)
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .When(x => !string.IsNullOrEmpty(x.WebsiteLink));
        }
    }
}