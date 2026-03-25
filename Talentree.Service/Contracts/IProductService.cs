using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Service.DTOs;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Products;

namespace Talentree.Service.Contracts
{
    public interface IProductService
    {
        Task<Pagination<ProductDto>> GetMyProductsAsync(string businessOwnerUserId, ProductFilterDto filter);
        Task<ProductDto> GetProductByIdAsync(int productId, string businessOwnerUserId);
        Task<ProductDto> CreateProductAsync(CreateProductDto dto, List<IFormFile> images, string businessOwnerUserId);
        Task<ProductDto> UpdateProductAsync(int productId, UpdateProductDto dto, List<IFormFile>? newImages, string businessOwnerUserId);
        Task DeleteProductAsync(int productId, string businessOwnerUserId);
    }
}
