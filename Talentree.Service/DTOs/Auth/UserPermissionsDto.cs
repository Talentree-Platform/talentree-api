using System.Collections.Generic;

namespace Talentree.Service.DTOs.Auth
{
    public class UserPermissionsDto
    {
        public string Role { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new List<string>();
    }
}
