using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.UserManagementSpecifications
{
    public class ComplaintsSpecification : BaseSpecifications<Complaint>
    {
        public ComplaintsSpecification(
            string? reportedUserId = null,
            ComplaintStatus? status = null,
            ViolationType? violationType = null,
            int? pageIndex = null,
            int? pageSize = null)
            : base(c =>
                (string.IsNullOrEmpty(reportedUserId) || c.ReportedUserId == reportedUserId) &&
                (!status.HasValue || c.Status == status.Value) &&
                (!violationType.HasValue || c.ViolationType == violationType.Value))
        {
            AddInclude(c => c.ReportedUser);
            AddInclude(c => c.ReportedBy);

            AddOrderByDescending(c => c.CreatedAt);

            if (pageIndex.HasValue && pageSize.HasValue)
            {
                var validPageIndex = pageIndex.Value < 1 ? 1 : pageIndex.Value;
                var validPageSize = pageSize.Value < 1 ? 20 : (pageSize.Value > 100 ? 100 : pageSize.Value);

                var skip = (validPageIndex - 1) * validPageSize;
                ApplyPagination(skip, validPageSize);
            }
        }
    }
}