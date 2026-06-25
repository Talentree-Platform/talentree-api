using System;
using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.UserManagementSpecifications
{
    public class AuditLogsSpecification : BaseSpecifications<UserActionLog>
    {
        public AuditLogsSpecification(string? adminId, string? action, DateTime? startDate, DateTime? endDate, int pageIndex, int pageSize)
            : base(l => 
                (string.IsNullOrEmpty(adminId) || l.AdminId == adminId) &&
                (string.IsNullOrEmpty(action) || l.Action == action) &&
                (!startDate.HasValue || l.ActionDate >= startDate.Value) &&
                (!endDate.HasValue || l.ActionDate <= endDate.Value)
            )
        {
            AddInclude(l => l.Admin);
            AddInclude(l => l.User);
            AddOrderByDescending(l => l.ActionDate);
            ApplyPagination(pageIndex, pageSize);
        }

        public AuditLogsSpecification(string? adminId, string? action, DateTime? startDate, DateTime? endDate)
            : base(l => 
                (string.IsNullOrEmpty(adminId) || l.AdminId == adminId) &&
                (string.IsNullOrEmpty(action) || l.Action == action) &&
                (!startDate.HasValue || l.ActionDate >= startDate.Value) &&
                (!endDate.HasValue || l.ActionDate <= endDate.Value)
            )
        {
            AddInclude(l => l.Admin);
            AddInclude(l => l.User);
            AddOrderByDescending(l => l.ActionDate);
        }
    }

    public class AuditLogsCountSpecification : BaseSpecifications<UserActionLog>
    {
        public AuditLogsCountSpecification(string? adminId, string? action, DateTime? startDate, DateTime? endDate)
            : base(l => 
                (string.IsNullOrEmpty(adminId) || l.AdminId == adminId) &&
                (string.IsNullOrEmpty(action) || l.Action == action) &&
                (!startDate.HasValue || l.ActionDate >= startDate.Value) &&
                (!endDate.HasValue || l.ActionDate <= endDate.Value)
            )
        {
        }
    }
}
