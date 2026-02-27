using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Service.DTOs.Admin.Supplier;
using Talentree.Service.DTOs.Common;

namespace Talentree.Service.Contracts
{
    /// <summary>
    /// Admin-only operations for managing suppliers.
    /// Suppliers are the source of raw materials on the platform.
    /// </summary>
    public interface ISupplierService
    {
        /// <summary>
        /// Gets a paginated list of all suppliers.
        /// Supports filtering by name/contact person and active status.
        /// </summary>
        /// <param name="search">Optional search term matched against supplier name and contact person</param>
        /// <param name="isActive">Optional filter — true for active only, false for inactive only</param>
        /// <param name="pageIndex">1-based page number</param>
        /// <param name="pageSize">Number of results per page (max 100)</param>
        Task<Pagination<SupplierDto>> GetSuppliersAsync(string? search, bool? isActive, int pageIndex, int pageSize);

        /// <summary>
        /// Gets full details of a single supplier including their material count.
        /// Throws <see cref="KeyNotFoundException"/> if not found.
        /// </summary>
        /// <param name="id">Supplier ID</param>
        Task<SupplierDto> GetSupplierByIdAsync(int id);

        /// <summary>
        /// Creates a new supplier. Email must be unique across all suppliers.
        /// Throws <see cref="InvalidOperationException"/> if email already exists.
        /// </summary>
        Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto dto);

        /// <summary>
        /// Partially updates a supplier. Only provided fields are updated.
        /// Throws <see cref="KeyNotFoundException"/> if not found.
        /// </summary>
        /// <param name="id">Supplier ID to update</param>
        Task<SupplierDto> UpdateSupplierAsync(int id, UpdateSupplierDto dto);

        /// <summary>
        /// Soft deletes a supplier and marks them as inactive.
        /// Throws <see cref="InvalidOperationException"/> if the supplier has active materials.
        /// Deactivate all their materials before deleting.
        /// </summary>
        /// <param name="id">Supplier ID to delete</param>
        Task DeleteSupplierAsync(int id);
    }
}
