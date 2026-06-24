using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Talentree.Core;
using Talentree.Core.Entities;
using Talentree.Service.Contracts;

namespace Talentree.Service.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogActionAsync(
            string? userId,
            string? adminId,
            string action,
            string reason,
            string? notes = null,
            string? entityType = null,
            string? entityId = null,
            string? beforeValues = null,
            string? afterValues = null)
        {
            var context = _httpContextAccessor.HttpContext;

            // Automatically resolve IP address
            string? ipAddress = context?.Connection?.RemoteIpAddress?.ToString();

            // Automatically resolve performing admin/user ID if not provided explicitly
            if (string.IsNullOrEmpty(adminId))
            {
                adminId = context?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            var log = new UserActionLog
            {
                UserId = userId,
                AdminId = adminId,
                Action = action,
                Reason = reason,
                Notes = notes,
                IpAddress = ipAddress,
                EntityType = entityType,
                EntityId = entityId,
                BeforeValues = beforeValues,
                AfterValues = afterValues,
                ActionDate = DateTime.UtcNow
            };

            _unitOfWork.Repository<UserActionLog>().Add(log);
            await _unitOfWork.CompleteAsync();
        }
    }
}
