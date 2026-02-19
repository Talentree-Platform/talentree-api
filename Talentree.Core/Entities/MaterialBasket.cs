using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talentree.Core.Entities
{
    public class MaterialBasket:BaseEntity
    {
        public string BusinessOwnerId { get; set; } = null!;
        public ICollection<MaterialBasketItem> Items { get; set; } = new List<MaterialBasketItem>();
    }
}
