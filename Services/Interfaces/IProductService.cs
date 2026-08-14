using EcommerceBackend.DTOs.Common;
using EcommerceBackend.DTOs.Product;

namespace EcommerceBackend.Services.Interfaces;

public interface IProductService
{
    Task<ApiResponse<List<ProductDto>>> GetAllAsync();

    Task<ApiResponse<ProductDto>> GetByIdAsync(Guid id);
    
     Task<ApiResponse<List<ProductDto>>> GetByCategoryAsync(
        Guid categoryId);

    Task<ApiResponse<ProductDto>> CreateAsync(
        CreateProductDto request);

    Task<ApiResponse<ProductDto>> UpdateAsync(
        Guid id,
        UpdateProductDto request);

    Task<ApiResponse<bool>> DeleteAsync(Guid id);
}