using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Core.DTOs.Admin.RawMaterial;
using Talentree.Core.DTOs.Common;

namespace Talentree.Core.Service.Contract
{
    public interface IAdminRawMaterialService
    {
        Task<PaginationDto<AdminRawMaterialDto>> GetMaterialsAsync(string? category, string? search, bool? isAvailable, int pageIndex, int pageSize);
        Task<AdminRawMaterialDto> GetMaterialByIdAsync(int id);
        Task<AdminRawMaterialDto> CreateMaterialAsync(CreateRawMaterialDto dto);
        Task<AdminRawMaterialDto> UpdateMaterialAsync(int id, UpdateRawMaterialDto dto);
        Task DeleteMaterialAsync(int id);
        Task<AdminRawMaterialDto> RestockMaterialAsync(int id, RestockMaterialDto dto);
        Task<string> UploadMaterialImageAsync(int id, IFormFile image);
    }
}
