using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.UserManagementSpecifications
{
    public class ComplaintsByUserSpecification : BaseSpecifications<Complaint>
    {
        public ComplaintsByUserSpecification(string userId)
            : base(c => c.ReportedUserId == userId && c.Status == ComplaintStatus.Resolved)
        {
        }
    }
}