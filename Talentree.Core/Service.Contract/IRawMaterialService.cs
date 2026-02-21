using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Core.DTOs.Common;
using Talentree.Core.DTOs.RawMaterial;

namespace Talentree.Core.Service.Contract
{
    /// <summary>
    /// Handles raw material browsing for Business Owners.
    /// Results are scoped to the BO's business category automatically.
    /// </summary>
    public interface IRawMaterialService
    {
        /// <summary>
        /// Gets a paginated list of available raw materials filtered by the BO's business category.
        /// Supports additional filtering by sub-category and free-text search.
        /// </summary>
        /// <param name="businessOwnerId">The authenticated BO's user ID — used to determine their business category</param>
        /// <param name="category">Optional sub-category filter</param>
        /// <param name="search">Optional search term matched against name and description</param>
        /// <param name="pageIndex">1-based page number</param>
        /// <param name="pageSize">Number of results per page (max 100)</param>
        Task<PaginationDto<RawMaterialDto>> GetMaterialsAsync(string businessOwnerId, string? category, string? search, int pageIndex, int pageSize);

        /// <summary>
        /// Gets full details of a single available raw material.
        /// Throws <see cref="KeyNotFoundException"/> if not found or unavailable.
        /// </summary>
        /// <param name="id">Material ID</param>
        Task<RawMaterialDto> GetMaterialByIdAsync(int id);
    }
}
