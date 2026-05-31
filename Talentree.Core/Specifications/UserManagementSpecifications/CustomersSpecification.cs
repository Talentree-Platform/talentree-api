using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.UserManagementSpecifications
{
    public class CustomersSpecification : BaseSpecifications<AppUser>
    {
        public CustomersSpecification(
            string? searchQuery = null,
            AccountStatus? accountStatus = null,
            DateTime? registrationDateFrom = null,
            DateTime? registrationDateTo = null,
            int? pageIndex = null,
            int? pageSize = null)
            : base(u =>
                u.BusinessOwnerProfile == null && // Not a business owner
                (string.IsNullOrEmpty(searchQuery) ||
                 u.DisplayName.Contains(searchQuery) ||
                 u.Email!.Contains(searchQuery)) &&
                (!accountStatus.HasValue || u.AccountStatus == accountStatus.Value) &&
                (!registrationDateFrom.HasValue || u.CreatedAt >= registrationDateFrom.Value) &&
                (!registrationDateTo.HasValue || u.CreatedAt <= registrationDateTo.Value))
        {
            AddOrderByDescending(u => u.CreatedAt);

            if (pageIndex.HasValue && pageSize.HasValue)
            {
                var validPageIndex = pageIndex.Value < 1 ? 1 : pageIndex.Value;
                var validPageSize = pageSize.Value < 1 ? 20 : (pageSize.Value > 100 ? 100 : pageSize.Value);

                ApplyPagination(validPageIndex, validPageSize);
            }
        }
    }
}