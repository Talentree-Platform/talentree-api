using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Core.DTOs.Common;
using Talentree.Core.DTOs.RawMaterial;

namespace Talentree.Core.Service.Contract
{
    public interface IRawMaterialService
    {
        Task<PaginationDto<RawMaterialDto>> GetMaterialsAsync(string businessOwnerId, string? category, string? search, int pageIndex, int pageSize);
        Task<RawMaterialDto> GetMaterialByIdAsync(int id);
    }
}
