using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talentree.Core.Enums
{
    public enum ApprovalStatus
    {
        /// <summary>
        /// Awaiting admin review
        /// </summary>
        Pending = 1,

        /// <summary>
        /// Approved by admin (manual or automatic)
        /// </summary>
        Approved = 2,

        /// <summary>
        /// Rejected by admin
        /// </summary>
        Rejected = 3
    }
}
