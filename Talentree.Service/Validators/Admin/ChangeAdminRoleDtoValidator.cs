using FluentValidation;
using System.Linq;
using Talentree.Service.DTOs.Admin;

namespace Talentree.Service.Validators.Admin
{
    public class ChangeAdminRoleDtoValidator : AbstractValidator<ChangeAdminRoleDto>
    {
        public ChangeAdminRoleDtoValidator()
        {
            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required")
                .Must(role => new[] { "SuperAdmin", "Admin", "SupportStaff", "ContentManager" }.Contains(role))
                .WithMessage("Invalid role. Role must be SuperAdmin, Admin, SupportStaff, or ContentManager");
        }
    }
}
