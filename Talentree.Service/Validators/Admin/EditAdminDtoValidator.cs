using FluentValidation;
using Talentree.Service.DTOs.Admin;

namespace Talentree.Service.Validators.Admin
{
    public class EditAdminDtoValidator : AbstractValidator<EditAdminDto>
    {
        public EditAdminDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Name is required")
                .MinimumLength(3).WithMessage("Name must be at least 3 characters")
                .MaximumLength(50).WithMessage("Name cannot exceed 50 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?\d{7,15}$")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("Invalid phone number format");
        }
    }
}
