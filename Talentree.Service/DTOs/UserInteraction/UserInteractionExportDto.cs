using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talentree.Service.DTOs.UserInteraction
{
    /// <summary>
    /// Export format for AI team (JSON format)
    /// </summary>
    public class UserInteractionExportDto
    {
        public string user_type { get; set; }
        public string user_id { get; set; }
        public int item_id { get; set; }
        public string item_type { get; set; }
        public string interaction_type { get; set; }
        public string category { get; set; }
        public int quantity { get; set; }
        public decimal price { get; set; }
        public string interaction_timestamp { get; set; }
    }
}
