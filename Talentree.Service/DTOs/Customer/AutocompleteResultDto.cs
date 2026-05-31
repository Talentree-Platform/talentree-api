using System.Collections.Generic;

namespace Talentree.Service.DTOs.Customer
{
    public class AutocompleteResultDto
    {
        public List<string> Suggestions { get; set; } = new();
    }
}
