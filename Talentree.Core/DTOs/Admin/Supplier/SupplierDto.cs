using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talentree.Core.DTOs.Admin.Supplier
{
    public class SupplierDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Country { get; set; } = null!;
        public string ContactPerson { get; set; } = null!;
        public string? TaxId { get; set; }
        public bool IsActive { get; set; }
        public int MaterialCount { get; set; }
    }

    public class CreateSupplierDto
    {
        [Required][MaxLength(200)] 
        public string Name { get; set; } = null!;
        
        [Required][MaxLength(1000)] 
        public string Description { get; set; } = null!;
       
        [Required][EmailAddress] 
        public string Email { get; set; } = null!;
        
        [Required][Phone]
        public string Phone { get; set; } = null!;
        
        [Required][MaxLength(500)] 
        public string Address { get; set; } = null!;
        
        [Required][MaxLength(100)] 
        public string City { get; set; } = null!;
        
        [Required][MaxLength(100)] 
        public string Country { get; set; } = null!;
        
        [Required][MaxLength(200)] 
        public string ContactPerson { get; set; } = null!;
       
        [MaxLength(100)] 
        public string? TaxId { get; set; }
    }

    public class UpdateSupplierDto
    {
        [MaxLength(200)] 
        public string? Name { get; set; }
      
        [MaxLength(1000)] 
        public string? Description { get; set; }
        
        [EmailAddress] 
        public string? Email { get; set; }
       
        [Phone] 
        public string? Phone { get; set; }
       
        [MaxLength(500)] 
        public string? Address { get; set; }
       
        [MaxLength(100)] 
        public string? City { get; set; }
        
        [MaxLength(100)] 
        public string? Country { get; set; }
        
        [MaxLength(200)] 
        public string? ContactPerson { get; set; }
        
        [MaxLength(100)] 
        public string? TaxId { get; set; }
        public bool? IsActive { get; set; }
    }
}
