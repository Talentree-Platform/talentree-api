using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.UserManagementSpecifications
{
    public class BusinessOwnersSpecification : BaseSpecifications<AppUser>
    {
        public BusinessOwnersSpecification(
            string? searchQuery = null,
            ApprovalStatus? status = null,
            AccountStatus? accountStatus = null,
            string? category = null,
            DateTime? registrationDateFrom = null,
            DateTime? registrationDateTo = null,
            int? pageIndex = null,
            int? pageSize = null)
            : base(u =>
                u.BusinessOwnerProfile != null &&
                (string.IsNullOrEmpty(searchQuery) ||
                 u.DisplayName.Contains(searchQuery) ||
                 u.Email!.Contains(searchQuery) ||
                 u.BusinessOwnerProfile.BusinessName.Contains(searchQuery)) &&
                (!status.HasValue || u.BusinessOwnerProfile.Status == status.Value) &&
                (!accountStatus.HasValue || u.AccountStatus == accountStatus.Value) &&
                (string.IsNullOrEmpty(category) || u.BusinessOwnerProfile.BusinessCategory == category) &&
                (!registrationDateFrom.HasValue || u.CreatedAt >= registrationDateFrom.Value) &&
                (!registrationDateTo.HasValue || u.CreatedAt <= registrationDateTo.Value))
        {
            AddInclude(u => u.BusinessOwnerProfile);

            AddOrderByDescending(u => u.CreatedAt);

            if (pageIndex.HasValue && pageSize.HasValue)
            {
                var validPageIndex = pageIndex.Value < 1 ? 1 : pageIndex.Value;
                var validPageSize = pageSize.Value < 1 ? 20 : (pageSize.Value > 100 ? 100 : pageSize.Value);

                //var skip = (validPageIndex - 1) * validPageSize;
                ApplyPagination(validPageIndex, validPageSize);
            }
        }
    }
}