using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talentree.Service.DTOs.UserInteraction
{
  
        public class UserInteractionDto
        {
            public int Id { get; set; }
            public string UserId { get; set; }
            public string UserType { get; set; }  // customer, owner, supplier
            public int ItemId { get; set; }
            public string ItemType { get; set; }  // product, raw_material
            public string ActionType { get; set; }  // view, click, purchase, reorder
            public string Category { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public DateTime InteractionTimestamp { get; set; }
            public DateTime CreatedAt { get; set; }
        }

  
}


