using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Service.DTOs.Admin.RawMaterial;
using Talentree.Service.DTOs.Common;

namespace Talentree.Service.Contracts
{
    /// <summary>
    /// Admin-only operations for managing raw materials including
    /// CRUD, stock management, and image uploads.
    /// </summary>
    public interface IAdminRawMaterialService
    {
        /// <summary>
        /// Gets a paginated list of all raw materials including unavailable ones.
        /// Supports filtering by category, search term, and availability status.
        /// </summary>
        /// <param name="category">Optional category filter</param>
        /// <param name="search">Optional search term matched against name and description</param>
        /// <param name="isAvailable">Optional availability filter</param>
        /// <param name="pageIndex">1-based page number</param>
        /// <param name="pageSize">Number of results per page (max 100)</param>
        Task<Pagination<AdminRawMaterialDto>> GetMaterialsAsync(string? category, string? search, bool? isAvailable, int pageIndex, int pageSize);

        /// <summary>
        /// Gets full details of a single raw material regardless of availability.
        /// Throws <see cref="KeyNotFoundException"/> if not found.
        /// </summary>
        /// <param name="id">Material ID</param>
        Task<AdminRawMaterialDto> GetMaterialByIdAsync(int id);

        /// <summary>
        /// Creates a new raw material linked to an active supplier.
        /// Availability is automatically set to true if initial stock is greater than zero.
        /// Throws <see cref="KeyNotFoundException"/> if the supplier is not found or inactive.
        /// </summary>
        Task<AdminRawMaterialDto> CreateMaterialAsync(CreateRawMaterialDto dto);

        /// <summary>
        /// Partially updates a raw material. Only provided fields are updated.
        /// Setting stock to 0 automatically marks the material as unavailable.
        /// Throws <see cref="KeyNotFoundException"/> if material or new supplier is not found.
        /// </summary>
        /// <param name="id">Material ID to update</param>
        Task<AdminRawMaterialDto> UpdateMaterialAsync(int id, UpdateRawMaterialDto dto);

        /// <summary>
        /// Soft deletes a raw material and marks it as unavailable.
        /// Throws <see cref="KeyNotFoundException"/> if not found.
        /// </summary>
        /// <param name="id">Material ID to delete</param>
        Task DeleteMaterialAsync(int id);

        /// <summary>
        /// Adds stock quantity to an existing material.
        /// Automatically re-enables availability if the material was out of stock.
        /// Throws <see cref="KeyNotFoundException"/> if not found.
        /// </summary>
        /// <param name="id">Material ID to restock</param>
        Task<AdminRawMaterialDto> RestockMaterialAsync(int id, RestockMaterialDto dto);

        /// <summary>
        /// Uploads or replaces the image for a raw material.
        /// Accepted formats: JPG, PNG, WebP. Maximum file size: 5MB.
        /// The old image file is deleted automatically when replaced.
        /// Returns the relative URL of the uploaded image.
        /// </summary>
        /// <param name="id">Material ID to upload image for</param>
        /// <param name="image">The image file from the multipart form</param>
        Task<string> UploadMaterialImageAsync(int id, IFormFile image);
    }
}
