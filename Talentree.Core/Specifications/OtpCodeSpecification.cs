using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Core.Entities.Identity;

namespace Talentree.Core.Specifications
{

    public class OtpCodeSpecification : BaseSpecifications<OtpCode>
    {

        public OtpCodeSpecification(string userId, string code, OtpPurpose purpose)
            : base(otp =>
                otp.UserId == userId &&
                otp.Code == code &&
                otp.Purpose == purpose &&
                !otp.IsUsed &&
                otp.ExpiresAt > DateTime.UtcNow
            )
        {
            AddInclude(otp => otp.User);
            AddOrderByDescending(otp => otp.CreatedAt);
        }

        public OtpCodeSpecification(string userId)
            : base(otp => otp.UserId == userId)
        {
            AddInclude(otp => otp.User);
            AddOrderByDescending(otp => otp.CreatedAt);
        }

        public OtpCodeSpecification(string userId, OtpPurpose purpose)
            : base(otp =>
                otp.UserId == userId &&
                otp.Purpose == purpose
            )
        {
            AddInclude(otp => otp.User);
            AddOrderByDescending(otp => otp.CreatedAt);
        }

        public static OtpCodeSpecification GetExpiredOtps()
        {

            return new OtpCodeSpecification(otp =>
                otp.ExpiresAt <= DateTime.UtcNow &&
                !otp.IsUsed
            );
        }

        // ═══════════════════════════════════════════════════════════
        // STATIC METHOD: Get all used OTPs older than X days
        // ═══════════════════════════════════════════════════════════

        public static OtpCodeSpecification GetUsedOtpsOlderThan(int daysOld)
        {
            //  Calculate cutoff date
            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);

            return new OtpCodeSpecification(otp =>
                otp.IsUsed &&
                otp.UsedAt.HasValue &&
                otp.UsedAt.Value <= cutoffDate
            );
        }

        // ═══════════════════════════════════════════════════════════
        // PRIVATE CONSTRUCTOR: For custom expressions (used by static methods)
        // ═══════════════════════════════════════════════════════════

        private OtpCodeSpecification(System.Linq.Expressions.Expression<Func<OtpCode, bool>> criteria)
            : base(criteria)
        {

        }
    }
}