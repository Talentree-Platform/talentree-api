using System;
using Talentree.Core.Entities.Identity;

namespace Talentree.Core.Specifications.AccountSettingsSpecifications
{
    public class LoginHistorySpecification : BaseSpecifications<LoginHistory>
    {
        public LoginHistorySpecification(string? userId, string? email, bool? isSuccessful, string? ipAddress, DateTime? startDate, DateTime? endDate, int pageIndex, int pageSize)
            : base(l =>
                (string.IsNullOrEmpty(userId) || l.UserId == userId) &&
                (string.IsNullOrEmpty(email) || (l.User != null && l.User.Email == email)) &&
                (!isSuccessful.HasValue || l.IsSuccessful == isSuccessful.Value) &&
                (string.IsNullOrEmpty(ipAddress) || l.IpAddress == ipAddress) &&
                (!startDate.HasValue || l.LoginAt >= startDate.Value) &&
                (!endDate.HasValue || l.LoginAt <= endDate.Value)
            )
        {
            AddInclude(l => l.User);
            AddOrderByDescending(l => l.LoginAt);
            ApplyPagination(pageIndex, pageSize);
        }

        public LoginHistorySpecification(string? userId, string? email, bool? isSuccessful, string? ipAddress, DateTime? startDate, DateTime? endDate)
            : base(l =>
                (string.IsNullOrEmpty(userId) || l.UserId == userId) &&
                (string.IsNullOrEmpty(email) || (l.User != null && l.User.Email == email)) &&
                (!isSuccessful.HasValue || l.IsSuccessful == isSuccessful.Value) &&
                (string.IsNullOrEmpty(ipAddress) || l.IpAddress == ipAddress) &&
                (!startDate.HasValue || l.LoginAt >= startDate.Value) &&
                (!endDate.HasValue || l.LoginAt <= endDate.Value)
            )
        {
            AddInclude(l => l.User);
            AddOrderByDescending(l => l.LoginAt);
        }
    }

    public class LoginHistoryCountSpecification : BaseSpecifications<LoginHistory>
    {
        public LoginHistoryCountSpecification(string? userId, string? email, bool? isSuccessful, string? ipAddress, DateTime? startDate, DateTime? endDate)
            : base(l =>
                (string.IsNullOrEmpty(userId) || l.UserId == userId) &&
                (string.IsNullOrEmpty(email) || (l.User != null && l.User.Email == email)) &&
                (!isSuccessful.HasValue || l.IsSuccessful == isSuccessful.Value) &&
                (string.IsNullOrEmpty(ipAddress) || l.IpAddress == ipAddress) &&
                (!startDate.HasValue || l.LoginAt >= startDate.Value) &&
                (!endDate.HasValue || l.LoginAt <= endDate.Value)
            )
        {
            AddInclude(l => l.User);
        }
    }
}
