using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Core.DTOs.Admin.Supplier;
using Talentree.Core.DTOs.Common;

namespace Talentree.Core.Service.Contract
{
    public interface ISupplierService
    {
        Task<PaginationDto<SupplierDto>> GetSuppliersAsync(string? search, bool? isActive, int pageIndex, int pageSize);
        Task<SupplierDto> GetSupplierByIdAsync(int id);
        Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto dto);
        Task<SupplierDto> UpdateSupplierAsync(int id, UpdateSupplierDto dto);
        Task DeleteSupplierAsync(int id);
    }
}
