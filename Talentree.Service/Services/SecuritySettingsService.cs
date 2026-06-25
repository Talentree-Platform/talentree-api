using AutoMapper;
using System.Text.Json;
using System.Threading.Tasks;
using Talentree.Core;
using Talentree.Core.Entities.Identity;
using Talentree.Core.Exceptions;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Admin;

namespace Talentree.Service.Services
{
    public class SecuritySettingsService : ISecuritySettingsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAuditLogService _auditLogService;

        public SecuritySettingsService(IUnitOfWork unitOfWork, IMapper mapper, IAuditLogService auditLogService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _auditLogService = auditLogService;
        }

        public async Task<SecuritySettingsDto> GetSecuritySettingsAsync()
        {
            var settings = await _unitOfWork.Repository<SecuritySettings>().GetByIdAsync(1);
            if (settings == null)
            {
                // Return defaults if not found in db
                settings = new SecuritySettings { Id = 1 };
            }
            return _mapper.Map<SecuritySettingsDto>(settings);
        }

        public async Task<SecuritySettingsDto> UpdateSecuritySettingsAsync(UpdateSecuritySettingsDto dto, string performingAdminId)
        {
            var settings = await _unitOfWork.Repository<SecuritySettings>().GetByIdAsync(1);
            bool isNew = false;
            if (settings == null)
            {
                settings = new SecuritySettings { Id = 1 };
                isNew = true;
            }

            // Serialize before values
            var beforeValues = JsonSerializer.Serialize(settings);

            // Apply updates with fail-safe checks (preventing 0 or negative values)
            settings.PasswordRequiredLength = dto.PasswordRequiredLength < 6 ? 8 : dto.PasswordRequiredLength;
            settings.PasswordRequireDigit = dto.PasswordRequireDigit;
            settings.PasswordRequireLowercase = dto.PasswordRequireLowercase;
            settings.PasswordRequireUppercase = dto.PasswordRequireUppercase;
            settings.PasswordRequireNonAlphanumeric = dto.PasswordRequireNonAlphanumeric;

            settings.SessionTimeoutInMinutes = dto.SessionTimeoutInMinutes <= 0 ? 15 : dto.SessionTimeoutInMinutes;
            settings.MaxFailedAccessAttempts = dto.MaxFailedAccessAttempts <= 0 ? 5 : dto.MaxFailedAccessAttempts;
            settings.LockoutDurationInMinutes = dto.LockoutDurationInMinutes <= 0 ? 15 : dto.LockoutDurationInMinutes;

            settings.RequireTwoFactorForAdmins = dto.RequireTwoFactorForAdmins;
            settings.IpWhitelist = dto.IpWhitelist;
            settings.AllowedLoginStartTime = dto.AllowedLoginStartTime;
            settings.AllowedLoginEndTime = dto.AllowedLoginEndTime;

            if (isNew)
            {
                _unitOfWork.Repository<SecuritySettings>().Add(settings);
            }
            else
            {
                _unitOfWork.Repository<SecuritySettings>().Update(settings);
            }

            await _unitOfWork.CompleteAsync();

            // Serialize after values
            var afterValues = JsonSerializer.Serialize(settings);

            // Log action in audit logs
            await _auditLogService.LogActionAsync(
                userId: null,
                adminId: performingAdminId,
                action: "Update Security Settings",
                reason: "SuperAdmin modified system-wide security settings",
                notes: "Updated session timeouts, password complexity, 2FA, lockout parameters, or IP whitelisting",
                entityType: "SecuritySettings",
                entityId: settings.Id.ToString(),
                beforeValues: beforeValues,
                afterValues: afterValues
            );

            return _mapper.Map<SecuritySettingsDto>(settings);
        }
    }
}
