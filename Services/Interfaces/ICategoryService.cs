using EcommerceBackend.DTOs.Category;
using EcommerceBackend.DTOs.Common;

namespace EcommerceBackend.Services.Interfaces;

public interface ICategoryService
{
    Task<ApiResponse<List<CategoryDto>>> GetAllAsync();

    Task<ApiResponse<CategoryDto>> GetByIdAsync(Guid id);

    Task<ApiResponse<CategoryDto>> CreateAsync(
        CreateCategoryDto request
    );

    Task<ApiResponse<CategoryDto>> UpdateAsync(
        Guid id,
        UpdateCategoryDto request
    );

    Task<ApiResponse<bool>> DeleteAsync(Guid id);
}