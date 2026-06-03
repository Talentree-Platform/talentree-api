using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talentree.Core.Enums
{
    
    /// <summary>
    /// Type of interaction performed
    /// </summary>
    public enum UserInteractionActionType
    {
        View = 0,      // User viewed product/material details
        Click = 1,     // User clicked on product/material
        Purchase = 2,  // User purchased product
        Reorder = 3    // User reordered raw material
    }   
}
