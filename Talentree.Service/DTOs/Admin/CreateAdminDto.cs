using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talentree.Service.DTOs.Admin
{
    /// <summary>
    /// DTO for creating a new admin user
    /// Only admins can use this endpoint
    /// </summary>
    public class CreateAdminDto
    {
     
        public string FullName { get; set; } = string.Empty;

        
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string ConfirmPassword { get; set; } = string.Empty;

   
        public string? PhoneNumber { get; set; }
    }
}
