using FluentValidation;
using Talentree.Service.DTOs.AccountSettings;

namespace Talentree.Service.Validators.AccountSettings
{
    public class UpsertPaymentInfoDtoValidator : AbstractValidator<UpsertPaymentInfoDto>
    {
        public UpsertPaymentInfoDtoValidator()
        {
            RuleFor(x => x.CurrentPassword).NotEmpty();
            RuleFor(x => x.BankName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.AccountHolderName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.AccountNumber)
                .NotEmpty()
                .Matches(@"^\d{8,20}$")
                .WithMessage("Account number must be numeric and between 8 and 20 digits");
            RuleFor(x => x.RoutingSwiftCode).NotEmpty().MaximumLength(20);
        }
    }
}
