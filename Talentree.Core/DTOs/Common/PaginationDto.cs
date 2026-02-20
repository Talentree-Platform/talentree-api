using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talentree.Core.DTOs.Common
{
    public class PaginationDto<T>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public List<T> Data { get; set; } = new();

        public PaginationDto(int pageIndex, int pageSize, int totalCount, List<T> data)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = totalCount;
            Data = data;
        }
    }
}
