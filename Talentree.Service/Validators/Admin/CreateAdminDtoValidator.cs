using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Service.DTOs.Admin;
using Talentree.Service.DTOs.Auth;

namespace Talentree.Service.Validators.Admin
{

    public class CreateAdminDtoValidator : AbstractValidator<CreateAdminDto>
    {
        public CreateAdminDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Name is required")
                .MinimumLength(3).WithMessage("Name must be at least 3 characters")
                .MaximumLength(50).WithMessage("Name cannot exceed 50 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%?&])[A-Za-z\d@$!%?&]{8,}$")
                .WithMessage("Password must be at least 8 characters and contain uppercase, lowercase, number and special character");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Confirm password is required")
                .Equal(x => x.Password).WithMessage("Passwords do not match");

            // Phone is optional but if provided, validate format
            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?\d{7,15}$")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("Invalid phone number format");
        }
    }
}
