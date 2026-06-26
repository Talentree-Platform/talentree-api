using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.UserManagementSpecifications
{
    public class DuplicateComplaintSpecification : BaseSpecifications<Complaint>
    {
        public DuplicateComplaintSpecification(
            string reporterUserId,
            string targetUserId,
            string? relatedProductId = null,
            string? relatedBrandId = null,
            string? relatedOrderId = null)
            : base(c =>
                c.ReportedByUserId == reporterUserId &&
                c.ReportedUserId == targetUserId &&
                c.Status == ComplaintStatus.Open &&
                (string.IsNullOrEmpty(relatedProductId) || c.RelatedProductId == relatedProductId) &&
                (string.IsNullOrEmpty(relatedBrandId) || c.RelatedBrandId == relatedBrandId) &&
                (string.IsNullOrEmpty(relatedOrderId) || c.RelatedOrderId == relatedOrderId))
        {
        }
    }
}
