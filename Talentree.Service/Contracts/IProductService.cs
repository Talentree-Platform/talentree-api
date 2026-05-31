using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Talentree.Service.DTOs;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Products;
using Talentree.Service.DTOs.Customer;

namespace Talentree.Service.Contracts
{
    public interface IProductService
    {
        Task<Pagination<ProductDto>> GetMyProductsAsync(string businessOwnerUserId, ProductFilterDto filter);
        Task<ProductDto> GetProductByIdAsync(int productId, string businessOwnerUserId);
        Task<ProductDto> CreateProductAsync(CreateProductDto dto, List<IFormFile> images, string businessOwnerUserId);
        Task<ProductDto> UpdateProductAsync(int productId, UpdateProductDto dto, List<IFormFile>? newImages, string businessOwnerUserId);
        Task DeleteProductAsync(int productId, string businessOwnerUserId);

        // Customer discovery endpoints
        Task<HomepageDto> GetHomepageDataAsync();
        Task<Pagination<CustomerProductDto>> SearchProductsAsync(CustomerProductFilterDto filter);
        Task<CustomerProductDetailDto> GetPublicProductByIdAsync(int productId);
        Task<Pagination<CustomerProductDto>> GetByCategoryAsync(int categoryId, CustomerProductFilterDto filter);
        Task<List<string>> GetAutocompleteAsync(string query);
        Task<Pagination<BrandListDto>> GetBrandsAsync(BrandFilterDto filter);
        Task<BrandDetailDto> GetBrandByIdAsync(int brandProfileId);
        Task<Pagination<CustomerProductDto>> GetBrandProductsAsync(int brandProfileId, CustomerProductFilterDto filter);
        Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync();
    }
}
