// Talentree.Service/Services/AdminService.cs

using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Talentree.Core;
using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;
using Talentree.Core.Exceptions;
using Talentree.Core.Specifications;
using Guidy.Core.Specifications;
using Talentree.Core.Specifications.BusinessOwnerSpecifications;
using Talentree.Core.Specifications.ProductSpecifications;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Admin;
using Talentree.Service.DTOs.Admin.Product;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Notification;

namespace Talentree.Service.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly INotificationHelperService _notificationHelper;
        private readonly ILogger<AdminService> _logger;
        private readonly IAuditLogService _auditLogService;

        public AdminService(
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            IEmailService emailService,
            IMapper mapper,
            INotificationService notificationService,
            INotificationHelperService notificationHelper,
            ILogger<AdminService> logger,
            IAuditLogService auditLogService
            )
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _emailService = emailService;
            _mapper = mapper;
            _notificationService = notificationService;
            _notificationHelper = notificationHelper;
            _logger = logger;
            _auditLogService = auditLogService;
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
            // ✅ ADD NOTIFICATION
            await _notificationHelper.NotifyBusinessOwnerApproved(profile.UserId);

            _logger.LogInformation("Business owner application {ProfileId} approved by admin {AdminId}",
                dto.ProfileId, adminUserId);
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
            // ✅ ADD NOTIFICATION
            await _notificationHelper.NotifyBusinessOwnerRejected(profile.UserId, dto.RejectionReason);

            _logger.LogInformation("Business owner application {ProfileId} rejected by admin {AdminId}. Reason: {Reason}",
                dto.ProfileId, adminUserId, dto.RejectionReason);
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

        public async Task<AdminDto> CreateAdminAsync(CreateAdminDto dto, string performingAdminId)
        {
            await VerifyPerformingAdminIsSuperAdminAsync(performingAdminId);

            // Check if email already exists
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
                CreatedAt = DateTime.UtcNow,
                MustChangePassword = true,
                IsTwoFactorEnabled = false
            };

            var tempPassword = GenerateTemporaryPassword();
            var result = await _userManager.CreateAsync(user, tempPassword);

            if (!result.Succeeded)
            {
                var errorsDict = result.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

                throw new ValidationException(errorsDict);
            }

            // Assign requested admin role
            await _userManager.AddToRoleAsync(user, dto.Role);

            // Welcome Email Body
            var emailBody = $@"
<div style=""font-family: 'Outfit', 'Inter', sans-serif; background-color: #f7f9fc; padding: 40px; border-radius: 12px; max-width: 600px; margin: 0 auto; color: #2c3e50;"">
  <div style=""text-align: center; margin-bottom: 30px;"">
    <h1 style=""color: #4a154b; font-size: 28px; font-weight: 700; margin: 0;"">Welcome to Talentree Admin Team</h1>
    <p style=""color: #7f8c8d; font-size: 16px; margin: 5px 0 0 0;"">Your administrative account has been successfully created</p>
  </div>
  
  <div style=""background-color: #ffffff; border-radius: 8px; padding: 30px; box-shadow: 0 4px 6px rgba(0,0,0,0.05);"">
    <p style=""font-size: 16px; line-height: 1.5; margin-top: 0;"">Hello <strong>{dto.FullName}</strong>,</p>
    <p style=""font-size: 15px; line-height: 1.5;"">You have been assigned the role of <strong style=""color: #4a154b;"">{dto.Role}</strong> on the Talentree Platform. Please find your login credentials below:</p>
    
    <div style=""background-color: #f1f2f6; border-left: 4px solid #4a154b; padding: 15px; margin: 20px 0; border-radius: 4px;"">
      <p style=""margin: 5px 0; font-size: 15px;""><strong>Login Email:</strong> {dto.Email}</p>
      <p style=""margin: 5px 0; font-size: 15px;""><strong>Temporary Password:</strong> <code style=""background-color: #fff; padding: 2px 6px; border-radius: 3px; font-weight: bold;"">{tempPassword}</code></p>
    </div>
    
    <p style=""font-size: 14px; color: #e74c3c; font-weight: bold; margin-bottom: 25px;"">Important: You will be required to change this password on your first login.</p>
    
    <div style=""text-align: center;"">
      <a href=""https://talentree.com/admin/login"" style=""background-color: #4a154b; color: #ffffff; padding: 12px 25px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block; font-size: 15px; box-shadow: 0 4px 6px rgba(74,21,75,0.2);"">Go to Admin Portal</a>
    </div>
  </div>
  
  <div style=""text-align: center; margin-top: 30px; font-size: 12px; color: #95a5a6;"">
    <p>Talentree Platform &copy; 2026. All rights reserved.</p>
  </div>
</div>";

            try
            {
                await _emailService.SendEmailAsync(user.Email!, "Welcome to Talentree - Admin Access Credentials", emailBody, isHtml: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome credentials email.");
            }

            // Audit Log
            var afterValues = $"{{\"displayName\": \"{user.DisplayName}\", \"email\": \"{user.Email}\", \"role\": \"{dto.Role}\", \"mustChangePassword\": true, \"isActive\": true}}";
            await LogAdminActionAsync(
                userId: user.Id,
                performingAdminId: performingAdminId,
                action: "Create Admin",
                reason: $"Admin account created with role: {dto.Role}",
                notes: null,
                entityType: "AppUser",
                entityId: user.Id,
                beforeValues: null,
                afterValues: afterValues
            );

            // ✅ ADD NOTIFICATION
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = user.Id,
                Type = NotificationType.Account,
                Title = "Welcome to Talentree Admin Team! 👋",
                Message = $"Your admin account has been created with the '{dto.Role}' role. You can now log in to the admin dashboard.",
                ActionUrl = "/admin/dashboard",
                ActionText = "Go to Dashboard",
                Priority = NotificationPriority.High,
                SendEmail = true,
                RelatedEntityType = "Account"
            });

            _logger.LogInformation("New admin created: {Email} with role {Role} by {PerformingAdminId}", dto.Email, dto.Role, performingAdminId);

            // Return admin details
            return MapToAdminDto(user, dto.Role);
        }

        public async Task<List<AdminDto>> GetAllAdminsAsync()
        {
            var allAdmins = new List<AdminDto>();

            var superAdmins = await _userManager.GetUsersInRoleAsync("SuperAdmin");
            allAdmins.AddRange(superAdmins.Select(u => MapToAdminDto(u, "SuperAdmin")));

            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            allAdmins.AddRange(admins.Select(u => MapToAdminDto(u, "Admin")));

            var supportStaff = await _userManager.GetUsersInRoleAsync("SupportStaff");
            allAdmins.AddRange(supportStaff.Select(u => MapToAdminDto(u, "SupportStaff")));

            var contentManagers = await _userManager.GetUsersInRoleAsync("ContentManager");
            allAdmins.AddRange(contentManagers.Select(u => MapToAdminDto(u, "ContentManager")));

            return allAdmins.OrderByDescending(a => a.CreatedAt).ToList();
        }

        public async Task DeactivateAdminAsync(string adminUserId, string performingAdminId)
        {
            await VerifyPerformingAdminIsSuperAdminAsync(performingAdminId);

            var admin = await _userManager.FindByIdAsync(adminUserId);

            if (admin == null)
                throw new NotFoundException("Admin not found");

            // Check if user is actually an admin
            var isAdmin = await IsAdminUserAsync(admin);
            if (!isAdmin)
                throw new BadRequestException("User is not an admin");

            if (adminUserId == performingAdminId)
                throw new BadRequestException("Cannot deactivate your own account");

            if (admin.Email == "projecttalentree@gmail.com")
                throw new BadRequestException("The primary emergency SuperAdmin account cannot be deactivated.");

            var superAdmins = await _userManager.GetUsersInRoleAsync("SuperAdmin");
            var activeSuperAdmins = superAdmins.Where(u => u.IsActive).ToList();
            if (activeSuperAdmins.Count <= 1 && activeSuperAdmins.Any(u => u.Id == adminUserId))
            {
                throw new BadRequestException("Cannot deactivate the last active SuperAdmin account. There must always be at least one active SuperAdmin in the system.");
            }

            // Deactivate
            admin.IsActive = false;
            await _userManager.UpdateAsync(admin);

            // Audit Log
            await LogAdminActionAsync(
                userId: adminUserId,
                performingAdminId: performingAdminId,
                action: "Deactivate Admin",
                reason: "Admin account deactivated",
                notes: null,
                entityType: "AppUser",
                entityId: adminUserId,
                beforeValues: "{\"isActive\": true}",
                afterValues: "{\"isActive\": false}"
            );

            // ✅ ADD NOTIFICATION
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = adminUserId,
                Type = NotificationType.Account,
                Title = "Account Deactivated 🔒",
                Message = "Your admin account has been deactivated. Please contact the system administrator if this was unexpected.",
                ActionUrl = "/support",
                ActionText = "Contact Support",
                Priority = NotificationPriority.High,
                SendEmail = true,
                RelatedEntityType = "Account"
            });

            _logger.LogInformation("Admin {AdminId} deactivated by {PerformingAdminId}", adminUserId, performingAdminId);
        }

        public async Task ReactivateAdminAsync(string adminUserId, string performingAdminId)
        {
            await VerifyPerformingAdminIsSuperAdminAsync(performingAdminId);

            var admin = await _userManager.FindByIdAsync(adminUserId);

            if (admin == null)
                throw new NotFoundException("Admin not found");

            var isAdmin = await IsAdminUserAsync(admin);
            if (!isAdmin)
                throw new BadRequestException("User is not an admin");

            // Reactivate
            admin.IsActive = true;
            await _userManager.UpdateAsync(admin);

            // Audit Log
            await LogAdminActionAsync(
                userId: adminUserId,
                performingAdminId: performingAdminId,
                action: "Reactivate Admin",
                reason: "Admin account reactivated",
                notes: null,
                entityType: "AppUser",
                entityId: adminUserId,
                beforeValues: "{\"isActive\": false}",
                afterValues: "{\"isActive\": true}"
            );

            // ✅ ADD NOTIFICATION
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = adminUserId,
                Type = NotificationType.Account,
                Title = "Account Reactivated ✅",
                Message = "Your admin account has been reactivated. You can now access the admin dashboard.",
                ActionUrl = "/admin/dashboard",
                ActionText = "Go to Dashboard",
                Priority = NotificationPriority.High,
                SendEmail = true,
                RelatedEntityType = "Account"
            });

            _logger.LogInformation("Admin {AdminId} reactivated by {PerformingAdminId}", adminUserId, performingAdminId);
        }

        public async Task<AdminDto> EditAdminAsync(string adminUserId, EditAdminDto dto, string performingAdminId)
        {
            await VerifyPerformingAdminIsSuperAdminAsync(performingAdminId);

            var admin = await _userManager.FindByIdAsync(adminUserId);
            if (admin == null)
                throw new NotFoundException("Admin not found");

            var isAdmin = await IsAdminUserAsync(admin);
            if (!isAdmin)
                throw new BadRequestException("User is not an admin");

            // Check if email already belongs to another user
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null && existingUser.Id != adminUserId)
                throw new BadRequestException("Email is already registered");

            // Update details
            var oldName = admin.DisplayName;
            var oldEmail = admin.Email;
            
            admin.DisplayName = dto.FullName;
            admin.Email = dto.Email;
            admin.UserName = dto.Email; // UserName matches Email
            admin.PhoneNumber = dto.PhoneNumber;

            var result = await _userManager.UpdateAsync(admin);
            if (!result.Succeeded)
            {
                var errorsDict = result.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
                throw new ValidationException(errorsDict);
            }

            // Audit Log
            var beforeValues = $"{{\"displayName\": \"{oldName}\", \"email\": \"{oldEmail}\"}}";
            var afterValues = $"{{\"displayName\": \"{dto.FullName}\", \"email\": \"{dto.Email}\"}}";
            await LogAdminActionAsync(
                userId: adminUserId,
                performingAdminId: performingAdminId,
                action: "Edit Admin",
                reason: $"Admin details updated. Old Name: '{oldName}', Old Email: '{oldEmail}' -> New Name: '{dto.FullName}', New Email: '{dto.Email}'",
                notes: null,
                entityType: "AppUser",
                entityId: adminUserId,
                beforeValues: beforeValues,
                afterValues: afterValues
            );

            var roles = await _userManager.GetRolesAsync(admin);
            var role = roles.FirstOrDefault() ?? "Admin";

            return MapToAdminDto(admin, role);
        }

        public async Task<AdminDto> ChangeAdminRoleAsync(string adminUserId, ChangeAdminRoleDto dto, string performingAdminId)
        {
            await VerifyPerformingAdminIsSuperAdminAsync(performingAdminId);

            var admin = await _userManager.FindByIdAsync(adminUserId);
            if (admin == null)
                throw new NotFoundException("Admin not found");

            var isAdmin = await IsAdminUserAsync(admin);
            if (!isAdmin)
                throw new BadRequestException("User is not an admin");

            if (adminUserId == performingAdminId)
                throw new BadRequestException("Super Admins cannot change their own roles.");

            if (admin.Email == "projecttalentree@gmail.com")
                throw new BadRequestException("The role of the primary emergency SuperAdmin account cannot be changed.");

            var currentRoles = await _userManager.GetRolesAsync(admin);

            if (currentRoles.Contains("SuperAdmin") && dto.Role != "SuperAdmin")
            {
                var superAdmins = await _userManager.GetUsersInRoleAsync("SuperAdmin");
                var activeSuperAdmins = superAdmins.Where(u => u.IsActive).ToList();
                if (activeSuperAdmins.Count <= 1 && activeSuperAdmins.Any(u => u.Id == adminUserId))
                {
                    throw new BadRequestException("Cannot demote the last active SuperAdmin account. There must always be at least one active SuperAdmin in the system.");
                }
            }

            var removeResult = await _userManager.RemoveFromRolesAsync(admin, currentRoles);
            if (!removeResult.Succeeded)
            {
                throw new BadRequestException("Failed to remove current roles");
            }

            var addResult = await _userManager.AddToRoleAsync(admin, dto.Role);
            if (!addResult.Succeeded)
            {
                throw new BadRequestException("Failed to assign the new role");
            }

            // Audit Log
            await LogAdminActionAsync(
                userId: adminUserId,
                performingAdminId: performingAdminId,
                action: "Change Role",
                reason: $"Changed role from '{string.Join(", ", currentRoles)}' to '{dto.Role}'",
                notes: null,
                entityType: "AppUser",
                entityId: adminUserId,
                beforeValues: $"{{\"role\": \"{string.Join(",", currentRoles)}\"}}",
                afterValues: $"{{\"role\": \"{dto.Role}\"}}"
            );

            _logger.LogInformation("Admin {AdminId} role changed from {OldRoles} to {NewRole} by {PerformingAdminId}", adminUserId, string.Join(", ", currentRoles), dto.Role, performingAdminId);

            return MapToAdminDto(admin, dto.Role);
        }

        public async Task ResetAdminPasswordAsync(string adminUserId, ResetAdminPasswordDto dto, string performingAdminId)
        {
            await VerifyPerformingAdminIsSuperAdminAsync(performingAdminId);

            var admin = await _userManager.FindByIdAsync(adminUserId);
            if (admin == null)
                throw new NotFoundException("Admin not found");

            var isAdmin = await IsAdminUserAsync(admin);
            if (!isAdmin)
                throw new BadRequestException("User is not an admin");

            var token = await _userManager.GeneratePasswordResetTokenAsync(admin);
            var result = await _userManager.ResetPasswordAsync(admin, token, dto.NewPassword);

            if (!result.Succeeded)
            {
                var errorsDict = result.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
                throw new ValidationException(errorsDict);
            }

            // Revoke active sessions for security
            var activeTokensSpec = new ActiveRefreshTokensForUserSpecification(admin.Id);
            var activeTokens = await _unitOfWork.Repository<RefreshToken>().GetAllWithSpecificationsAsync(activeTokensSpec);

            foreach (var rToken in activeTokens)
            {
                rToken.RevokedAt = DateTime.UtcNow;
                _unitOfWork.Repository<RefreshToken>().Update(rToken);
            }
            await _unitOfWork.CompleteAsync();

            // Audit Log
            await LogAdminActionAsync(
                userId: adminUserId,
                performingAdminId: performingAdminId,
                action: "Password Changed",
                reason: "Admin password reset by Super Admin",
                notes: null,
                entityType: "AppUser",
                entityId: adminUserId,
                beforeValues: null,
                afterValues: "{\"passwordReset\": true}"
            );

            // Notify user
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = adminUserId,
                Type = NotificationType.Account,
                Title = "Password Reset by Administrator 🔒",
                Message = "Your password has been reset by the Super Administrator. Please use your new temporary credentials.",
                ActionUrl = "/login",
                ActionText = "Log In",
                Priority = NotificationPriority.High,
                SendEmail = true,
                RelatedEntityType = "Account"
            });

            _logger.LogInformation("Admin {AdminId} password reset by {PerformingAdminId}", adminUserId, performingAdminId);
        }

        // Helpers
        private async Task<bool> IsAdminUserAsync(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var adminRoles = new[] { "SuperAdmin", "Admin", "SupportStaff", "ContentManager" };
            return roles.Intersect(adminRoles).Any();
        }

        private AdminDto MapToAdminDto(AppUser user, string role)
        {
            return new AdminDto
            {
                Id = user.Id,
                FullName = user.DisplayName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                Role = role
            };
        }

        private async Task VerifyPerformingAdminIsSuperAdminAsync(string performingAdminId)
        {
            var performer = await _userManager.FindByIdAsync(performingAdminId);
            if (performer == null)
            {
                throw new UnauthorizedException("Performing admin not found");
            }

            var performerRoles = await _userManager.GetRolesAsync(performer);
            if (!performerRoles.Contains("Admin"))
            {
                throw new ForbiddenException("Privilege escalation prevented: Only SuperAdmin can perform admin management actions.");
            }
        }

        private async Task LogAdminActionAsync(string? userId, string? performingAdminId, string action, string reason, string? notes, string? entityType = null, string? entityId = null, string? beforeValues = null, string? afterValues = null)
        {
            await _auditLogService.LogActionAsync(userId, performingAdminId, action, reason, notes, entityType, entityId, beforeValues, afterValues);
        }


        public async Task<Pagination<PendingProductDto>> GetPendingProductsAsync(
      int pageIndex = 1, int pageSize = 20)
        {
            // Validate pagination
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            // Count
            var countSpec = new PendingProductsSpecification();
            var totalCount = await _unitOfWork.Repository<Core.Entities.Product>()
                .GetCountWithSpecificationsAsync(countSpec);

            // Get paginated
            var spec = new PendingProductsSpecification(pageIndex, pageSize);
            var products = await _unitOfWork.Repository<Core.Entities.Product>()
                .GetAllWithSpecificationsAsync(spec);

            var dtos = _mapper.Map<List<PendingProductDto>>(products);

            return new Pagination<PendingProductDto>(
                pageIndex, pageSize, totalCount, dtos);
        }
        /// <summary>
        /// Approve product
        /// </summary>
        public async Task ApproveProductAsync(ApproveProductDto dto, string adminId)
        {
            // Get product with owner
            var spec = new ProductByIdSpecification(dto.ProductId);
            var product = await _unitOfWork.Repository<Core.Entities.Product>()
                .GetByIdWithSpecificationsAsync(spec);

            if (product == null)
                throw new NotFoundException("Product not found");

            if (product.Status != ProductStatus.PendingApproval)
                throw new BadRequestException("Product is not pending approval");

            // Approve product
            product.Status = ProductStatus.Approved;
            product.ApprovedAt = DateTime.UtcNow;
            product.ApprovedBy = adminId;
            product.RejectionReason = null;

            _unitOfWork.Repository<Core.Entities.Product>().Update(product);
            await _unitOfWork.CompleteAsync();

            await LogAdminActionAsync(
                userId: product.BusinessOwner.UserId,
                performingAdminId: adminId,
                action: "Approve Product",
                reason: $"Product '{product.Name}' approved.",
                notes: null,
                entityType: "Product",
                entityId: product.Id.ToString()
            );

            // ⭐ Send notification to business owner
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = product.BusinessOwner.UserId,
                Type = NotificationType.Product,
                Title = "Product Approved! ",
                Message = $"Great news! Your product '{product.Name}' has been approved and is now live on Talentree.",
                ActionUrl = $"/products/{product.Id}",
                ActionText = "View Product",
                RelatedEntityType = "Product",
                RelatedEntityId = product.Id,
                Priority = NotificationPriority.High,
                SendEmail = true
            });



            _logger.LogInformation("Product {ProductId} approved by admin {AdminId}. Owner: {OwnerId}",
                dto.ProductId, adminId, product.BusinessOwner.UserId);
        }

        /// <summary>
        /// Reject product
        /// </summary>
        public async Task RejectProductAsync(RejectProductDto dto, string adminId)
        {
            // Get product with owner
            var spec = new ProductByIdSpecification(dto.ProductId);
            var product = await _unitOfWork.Repository<Core.Entities.Product>()
                .GetByIdWithSpecificationsAsync(spec);

            if (product == null)
                throw new NotFoundException("Product not found");

            if (product.Status != ProductStatus.PendingApproval)
                throw new BadRequestException("Product is not pending approval");

            // Reject product
            product.Status = ProductStatus.Rejected;
            product.ApprovedAt = null;
            product.ApprovedBy = null;
            product.RejectionReason = dto.Reason;

            _unitOfWork.Repository<Core.Entities.Product>().Update(product);
            await _unitOfWork.CompleteAsync();

            await LogAdminActionAsync(
                userId: product.BusinessOwner.UserId,
                performingAdminId: adminId,
                action: "Reject Product",
                reason: $"Product '{product.Name}' rejected. Reason: {dto.Reason}",
                notes: null,
                entityType: "Product",
                entityId: product.Id.ToString()
            );

            // ⭐ Send notification to business owner
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = product.BusinessOwner.UserId,
                Type = NotificationType.Product,
                Title = "Product Needs Revision",
                Message = $"Your product '{product.Name}' requires some changes before approval. Reason: {dto.Reason}",
                ActionUrl = $"/products/{product.Id}/edit",
                ActionText = "Edit Product",
                RelatedEntityType = "Product",
                RelatedEntityId = product.Id,
                Priority = NotificationPriority.High,
                SendEmail = true
            });



            _logger.LogInformation("Product {ProductId} rejected by admin {AdminId}. Reason: {Reason}",
                dto.ProductId, adminId, dto.Reason);
        }

        private string GenerateTemporaryPassword()
        {
            var lowercase = "abcdefghijklmnopqrstuvwxyz";
            var uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var digits = "0123456789";
            var nonAlphanumeric = "!@#$%^&*()_+";
            
            var random = new Random();
            var password = new char[12];
            password[0] = lowercase[random.Next(lowercase.Length)];
            password[1] = uppercase[random.Next(uppercase.Length)];
            password[2] = digits[random.Next(digits.Length)];
            password[3] = nonAlphanumeric[random.Next(nonAlphanumeric.Length)];
            
            var allChars = lowercase + uppercase + digits + nonAlphanumeric;
            for (int i = 4; i < 12; i++)
            {
                password[i] = allChars[random.Next(allChars.Length)];
            }
            
            return new string(password.OrderBy(_ => random.Next()).ToArray());
        }

        public async Task RevokeSessionsAsync(string adminUserId, string performingAdminId)
        {
            await VerifyPerformingAdminIsSuperAdminAsync(performingAdminId);

            var admin = await _userManager.FindByIdAsync(adminUserId);
            if (admin == null)
                throw new NotFoundException("Admin not found");

            var isAdmin = await IsAdminUserAsync(admin);
            if (!isAdmin)
                throw new BadRequestException("User is not an admin");

            var activeTokensSpec = new ActiveRefreshTokensForUserSpecification(admin.Id);
            var activeTokens = await _unitOfWork.Repository<RefreshToken>().GetAllWithSpecificationsAsync(activeTokensSpec);

            foreach (var rToken in activeTokens)
            {
                rToken.RevokedAt = DateTime.UtcNow;
                _unitOfWork.Repository<RefreshToken>().Update(rToken);
            }
            await _unitOfWork.CompleteAsync();

            // Audit log
            await LogAdminActionAsync(
                userId: adminUserId,
                performingAdminId: performingAdminId,
                action: "Session Revoked",
                reason: "All active refresh tokens revoked by Super Admin",
                notes: null,
                entityType: "AppUser",
                entityId: adminUserId,
                beforeValues: $"{{\"activeSessionsCount\": {activeTokens.Count}}}",
                afterValues: "{\"activeSessionsCount\": 0}"
            );
        }

        public async Task UnlockAdminAsync(string adminUserId, string performingAdminId)
        {
            await VerifyPerformingAdminIsSuperAdminAsync(performingAdminId);

            var admin = await _userManager.FindByIdAsync(adminUserId);
            if (admin == null)
                throw new NotFoundException("Admin not found");

            var isAdmin = await IsAdminUserAsync(admin);
            if (!isAdmin)
                throw new BadRequestException("User is not an admin");

            // Reset fail count
            var lockoutEndBefore = admin.LockoutEnd;
            var accessFailedCountBefore = admin.AccessFailedCount;

            await _userManager.ResetAccessFailedCountAsync(admin);
            await _userManager.SetLockoutEndDateAsync(admin, null);

            // Audit action
            await LogAdminActionAsync(
                userId: adminUserId,
                performingAdminId: performingAdminId,
                action: "Account Unlocked",
                reason: "Account unlocked by Super Admin",
                notes: null,
                entityType: "AppUser",
                entityId: adminUserId,
                beforeValues: $"{{\"lockoutEnd\": \"{lockoutEndBefore}\", \"accessFailedCount\": {accessFailedCountBefore}}}",
                afterValues: "{\"lockoutEnd\": null, \"accessFailedCount\": 0}"
            );
        }
    }
}