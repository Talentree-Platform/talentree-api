// Talentree.Service/Services/AdminService.cs

using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Talentree.Core;
using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;
using Talentree.Core.Exceptions;
using Talentree.Core.Specifications.BusinessOwnerSpecifications;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs;
using Talentree.Service.DTOs.Admin;

namespace Talentree.Service.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public AdminService(
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            IEmailService emailService,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _emailService = emailService;
            _mapper = mapper;
        }

        public async Task<Pagination<BusinessOwnerApplicationDto>> GetPendingBusinessOwnersAsync(
            int pageIndex = 1,
            int pageSize = 20)
        {
            // Validate pagination parameters
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100; // Max page size

            // Get total count (without pagination)
            var countSpec = new PendingBusinessOwnersSpecification();
            var totalCount = await _unitOfWork.Repository<BusinessOwnerProfile>()
                .GetCountWithSpecificationsAsync(countSpec);

            // Get paginated data
            var spec = new PendingBusinessOwnersSpecification(pageIndex, pageSize);
            var profiles = await _unitOfWork.Repository<BusinessOwnerProfile>()
                .GetAllWithSpecificationsAsync(spec);

            // Map to DTOs
            var dtos = _mapper.Map<List<BusinessOwnerApplicationDto>>(profiles);

            // Return pagination result
            return new Pagination<BusinessOwnerApplicationDto>(
                pageIndex: pageIndex,
                pageSize: pageSize,
                count: totalCount,
                data: dtos
            );
        }

        public async Task<BusinessOwnerApplicationDto> GetBusinessOwnerDetailsAsync(int profileId)
        {
            var spec = new BusinessOwnerProfileByIdSpecification(profileId);
            var profile = await _unitOfWork.Repository<BusinessOwnerProfile>()
                .GetByIdWithSpecificationsAsync(spec);

            if (profile == null)
                throw new NotFoundException("Business owner profile not found");

            return _mapper.Map<BusinessOwnerApplicationDto>(profile);
        }

        public async Task ApproveBusinessOwnerAsync(ApproveBusinessOwnerDto dto, string adminUserId)
        {
            var spec = new BusinessOwnerProfileByIdSpecification(dto.ProfileId);
            var profile = await _unitOfWork.Repository<BusinessOwnerProfile>()
                .GetByIdWithSpecificationsAsync(spec);

            if (profile == null)
                throw new NotFoundException("Business owner profile not found");

            if (profile.Status != ApprovalStatus.Pending)
                throw new BadRequestException($"Cannot approve application with status: {profile.Status}");

            profile.Status = ApprovalStatus.Approved;
            profile.ApprovedAt = DateTime.UtcNow;
            profile.ApprovedBy = adminUserId;
            profile.RejectionReason = null;
            profile.AutoApprovalDeadline = null;

            _unitOfWork.Repository<BusinessOwnerProfile>().Update(profile);
            await _unitOfWork.CompleteAsync();

            await SendApprovalEmailAsync(profile.User);
        }

        public async Task RejectBusinessOwnerAsync(RejectBusinessOwnerDto dto, string adminUserId)
        {
            if (string.IsNullOrWhiteSpace(dto.RejectionReason))
                throw new BadRequestException("Rejection reason is required");

            var spec = new BusinessOwnerProfileByIdSpecification(dto.ProfileId);
            var profile = await _unitOfWork.Repository<BusinessOwnerProfile>()
                .GetByIdWithSpecificationsAsync(spec);

            if (profile == null)
                throw new NotFoundException("Business owner profile not found");

            if (profile.Status != ApprovalStatus.Pending)
                throw new BadRequestException($"Cannot reject application with status: {profile.Status}");

            profile.Status = ApprovalStatus.Rejected;
            profile.RejectionReason = dto.RejectionReason;
            profile.ApprovedBy = adminUserId;
            profile.ApprovedAt = DateTime.UtcNow;
            profile.AutoApprovalDeadline = null;

            _unitOfWork.Repository<BusinessOwnerProfile>().Update(profile);
            await _unitOfWork.CompleteAsync();

            await SendRejectionEmailAsync(profile.User, dto.RejectionReason);
        }

        // ... (email methods remain the same)

        private async Task SendApprovalEmailAsync(AppUser user)
        {
            var subject = "Business Owner Application Approved - Talentree";
            var body = GetApprovalEmailTemplate(user.DisplayName);
            await _emailService.SendEmailAsync(user.Email!, subject, body);
        }

        private async Task SendRejectionEmailAsync(AppUser user, string reason)
        {
            var subject = "Business Owner Application Status - Talentree";
            var body = GetRejectionEmailTemplate(user.DisplayName, reason);
            await _emailService.SendEmailAsync(user.Email!, subject, body);
        }

        private string GetApprovalEmailTemplate(string userName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }}
        .email-container {{ max-width: 600px; margin: 50px auto; background-color: #ffffff; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .email-header {{ text-align: center; color: #4CAF50; margin-bottom: 20px; }}
        h1 {{ margin: 0; font-size: 28px; }}
        .success-box {{ background-color: #d4edda; border-left: 4px solid #4CAF50; padding: 15px; margin: 20px 0; border-radius: 5px; }}
        .cta-button {{ display: inline-block; background-color: #4CAF50; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin-top: 20px; }}
        .footer {{ text-align: center; color: #777; font-size: 12px; margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='email-header'>
            <h1>🎉 Application Approved!</h1>
        </div>
        <p>Hi {userName},</p>
        <div class='success-box'>
            <p><strong>Congratulations!</strong> Your business owner application has been approved.</p>
        </div>
        <p>You now have full access to the Talentree business owner dashboard.</p>
        <div class='footer'>
            <p><strong>Talentree Platform</strong></p>
            <p>© 2026 Talentree. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetRejectionEmailTemplate(string userName, string reason)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }}
        .email-container {{ max-width: 600px; margin: 50px auto; background-color: #ffffff; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .email-header {{ text-align: center; color: #f44336; margin-bottom: 20px; }}
        h1 {{ margin: 0; font-size: 28px; }}
        .warning-box {{ background-color: #f8d7da; border-left: 4px solid #f44336; padding: 15px; margin: 20px 0; border-radius: 5px; }}
        .footer {{ text-align: center; color: #777; font-size: 12px; margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='email-header'>
            <h1>Application Status Update</h1>
        </div>
        <p>Hi {userName},</p>
        <p>Thank you for your interest in becoming a business owner on Talentree.</p>
        <div class='warning-box'>
            <p><strong>Application Status:</strong> Not Approved</p>
            <p><strong>Reason:</strong> {reason}</p>
        </div>
        <div class='footer'>
            <p><strong>Talentree Platform</strong></p>
            <p>© 2026 Talentree. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }



        // ═══════════════════════════════════════════════════════════
        // ADMIN MANAGEMENT METHODS
        // ═══════════════════════════════════════════════════════════

        public async Task<AdminDto> CreateAdminAsync(CreateAdminDto dto)
        {
            //  Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new BadRequestException("Email is already registered");

            // Create new user
            var user = new AppUser
            {
                DisplayName = dto.FullName,
                Email = dto.Email,
                UserName = dto.Email, // Email as username
                PhoneNumber = dto.PhoneNumber,
                EmailConfirmed = true, 
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                var errorsDict = result.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

                throw new ValidationException(errorsDict);
            }

            // 3 Assign Admin role
            await _userManager.AddToRoleAsync(user, "Admin");

            // Return admin details
            return new AdminDto
            {
                Id = user.Id,
                FullName = user.DisplayName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<List<AdminDto>> GetAllAdminsAsync()
        {
            // Get all users with Admin role
            var admins = await _userManager.GetUsersInRoleAsync("Admin");

            return admins.Select(u => new AdminDto
            {
                Id = u.Id,
                FullName = u.DisplayName,
                Email = u.Email!,
                PhoneNumber = u.PhoneNumber,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            }).ToList();
        }

        public async Task DeactivateAdminAsync(string adminUserId)
        {
            var admin = await _userManager.FindByIdAsync(adminUserId);

            if (admin == null)
                throw new NotFoundException("Admin not found");

            // Check if user is actually an admin
            var isAdmin = await _userManager.IsInRoleAsync(admin, "Admin");
            if (!isAdmin)
                throw new BadRequestException("User is not an admin");

            // Deactivate
            admin.IsActive = false;
            await _userManager.UpdateAsync(admin);
        }

        public async Task ReactivateAdminAsync(string adminUserId)
        {
            var admin = await _userManager.FindByIdAsync(adminUserId);

            if (admin == null)
                throw new NotFoundException("Admin not found");

            var isAdmin = await _userManager.IsInRoleAsync(admin, "Admin");
            if (!isAdmin)
                throw new BadRequestException("User is not an admin");

            // Reactivate
            admin.IsActive = true;
            await _userManager.UpdateAsync(admin);
        }
    }
}