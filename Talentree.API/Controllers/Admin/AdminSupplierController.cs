using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talentree.API.Models;
using Talentree.Core.DTOs.Admin.Supplier;
using Talentree.Core.Service.Contract;

namespace Talentree.API.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminSupplierController : BaseApiController
    {
        private readonly ISupplierService _supplierService;

        public AdminSupplierController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        // GET: api/admin/suppliers?search=cotton&isActive=true&pageIndex=1&pageSize=20
        [HttpGet]
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

        // GET: api/admin/suppliers/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetSupplierById(int id)
        {
            var supplier = await _supplierService.GetSupplierByIdAsync(id);

            return Ok(ApiResponse<SupplierDto>.SuccessResponse(
                data: supplier,
                message: "Supplier retrieved successfully."
            ));
        }

        // POST: api/admin/suppliers
        [HttpPost]
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

        // PUT: api/admin/suppliers/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateSupplier(int id, [FromBody] UpdateSupplierDto dto)
        {
            var supplier = await _supplierService.UpdateSupplierAsync(id, dto);

            return Ok(ApiResponse<SupplierDto>.SuccessResponse(
                data: supplier,
                message: "Supplier updated successfully."
            ));
        }

        // DELETE: api/admin/suppliers/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            await _supplierService.DeleteSupplierAsync(id);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Supplier deleted successfully."
            ));
        }
    }
}
