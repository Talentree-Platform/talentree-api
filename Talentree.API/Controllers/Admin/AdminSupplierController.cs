using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talentree.API.Models;
using Talentree.Service.DTOs.Admin.Supplier;
using Talentree.Service.Contracts;

namespace Talentree.API.Controllers.Admin
{
    /// <summary>
    /// Admin-only endpoints for managing suppliers.
    /// Suppliers are the source of raw materials on the platform.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminSupplierController : BaseApiController
    {
        private readonly ISupplierService _supplierService;

        public AdminSupplierController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        /// <summary>
        /// Gets a paginated list of all suppliers.
        /// Supports filtering by name/contact person and active status.
        /// </summary>
        /// <param name="search">Optional search term matched against supplier name and contact person</param>
        /// <param name="isActive">Optional filter — true for active suppliers only, false for inactive</param>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetSuppliers(
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20)
        {
            if (pageIndex < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid pagination parameters."));

            var result = await _supplierService.GetSuppliersAsync(search, isActive, pageIndex, pageSize);

            return Ok(ApiResponse<object>.SuccessResponse(
                data: result,
                message: $"Retrieved {result.Data.Count} suppliers."
            ));
        }

        /// <summary>
        /// Gets full details of a single supplier including their material count.
        /// </summary>
        /// <param name="id">Supplier ID</param>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<SupplierDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSupplierById(int id)
        {
            var supplier = await _supplierService.GetSupplierByIdAsync(id);

            return Ok(ApiResponse<SupplierDto>.SuccessResponse(
                data: supplier,
                message: "Supplier retrieved successfully."
            ));
        }

        /// <summary>
        /// Creates a new supplier.
        /// Email must be unique across all suppliers.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<SupplierDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierDto dto)
        {
            var supplier = await _supplierService.CreateSupplierAsync(dto);

            return CreatedAtAction(
                nameof(GetSupplierById),
                new { id = supplier.Id },
                ApiResponse<SupplierDto>.SuccessResponse(
                    data: supplier,
                    message: $"Supplier '{supplier.Name}' created successfully."
                ));
        }

        /// <summary>
        /// Partially updates a supplier. Only provided fields are updated.
        /// </summary>
        /// <param name="id">Supplier ID to update</param>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<SupplierDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSupplier(int id, [FromBody] UpdateSupplierDto dto)
        {
            var supplier = await _supplierService.UpdateSupplierAsync(id, dto);

            return Ok(ApiResponse<SupplierDto>.SuccessResponse(
                data: supplier,
                message: "Supplier updated successfully."
            ));
        }

        /// <summary>
        /// Soft deletes a supplier.
        /// Will fail if the supplier has active materials — deactivate their materials first.
        /// </summary>
        /// <param name="id">Supplier ID to delete</param>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            await _supplierService.DeleteSupplierAsync(id);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Supplier deleted successfully."
            ));
        }
    }
}
