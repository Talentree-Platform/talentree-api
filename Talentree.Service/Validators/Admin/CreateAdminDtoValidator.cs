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

            // Phone is optional but if provided, validate format
            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?\d{7,15}$")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("Invalid phone number format");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required")
                .Must(role => new[] { "SuperAdmin", "Admin", "SupportStaff", "ContentManager" }.Contains(role))
                .WithMessage("Invalid role. Role must be SuperAdmin, Admin, SupportStaff, or ContentManager");
        }
    }
}
