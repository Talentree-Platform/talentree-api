using Talentree.Core.Entities.Identity;

namespace Talentree.Core.Specifications.AccountSettingsSpecifications
{
    public class PaymentInfoByUserSpecification : BaseSpecifications<BusinessOwnerPaymentInfo>
    {
        public PaymentInfoByUserSpecification(string userId)
            : base(p => p.UserId == userId)
        {
        }
    }
}