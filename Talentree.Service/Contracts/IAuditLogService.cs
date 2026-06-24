using System.Threading.Tasks;

namespace Talentree.Service.Contracts
{
    public interface IAuditLogService
    {
        Task LogActionAsync(
            string? userId,
            string? adminId,
            string action,
            string reason,
            string? notes = null,
            string? entityType = null,
            string? entityId = null,
            string? beforeValues = null,
            string? afterValues = null);
    }
}
