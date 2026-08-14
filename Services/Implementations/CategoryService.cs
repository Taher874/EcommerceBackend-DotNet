using EcommerceBackend.Data;
using EcommerceBackend.DTOs.Category;
using EcommerceBackend.DTOs.Common;
using EcommerceBackend.Models;
using EcommerceBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    // ==========================================
    // GET ALL
    // ==========================================

    public async Task<ApiResponse<List<CategoryDto>>> GetAllAsync()
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new CategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                ImageUrl = x.ImageUrl,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();

        return ApiResponse<List<CategoryDto>>.SuccessResponse(
            categories,
            "Categories fetched successfully."
        );
    }

    // ==========================================
    // GET BY ID
    // ==========================================

    public async Task<ApiResponse<CategoryDto>> GetByIdAsync(Guid id)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .Where(x => x.Id == id && x.IsActive)
            .Select(x => new CategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                ImageUrl = x.ImageUrl,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (category == null)
        {
            return ApiResponse<CategoryDto>.FailureResponse(
                "Category not found."
            );
        }

        return ApiResponse<CategoryDto>.SuccessResponse(
            category,
            "Category fetched successfully."
        );
    }

    // ==========================================
    // CREATE
    // ==========================================

    public async Task<ApiResponse<CategoryDto>> CreateAsync(
        CreateCategoryDto request)
    {
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return ApiResponse<CategoryDto>.FailureResponse(
                "Category name is required."
            );
        }

        var exists = await _context.Categories
            .AnyAsync(x =>
                x.Name.ToLower() == name.ToLower()
            );

        if (exists)
        {
            return ApiResponse<CategoryDto>.FailureResponse(
                "Category with this name already exists."
            );
        }

        var category = new Category
        {
            Id = Guid.NewGuid(),

            Name = name,

            Description = string.IsNullOrWhiteSpace(
                request.Description)
                ? null
                : request.Description.Trim(),

            ImageUrl = string.IsNullOrWhiteSpace(
                request.ImageUrl)
                ? null
                : request.ImageUrl.Trim(),

            IsActive = true,

            CreatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);

        await _context.SaveChangesAsync();

        return ApiResponse<CategoryDto>.SuccessResponse(
            MapToDto(category),
            "Category created successfully."
        );
    }

    // ==========================================
    // UPDATE
    // ==========================================

    public async Task<ApiResponse<CategoryDto>> UpdateAsync(
        Guid id,
        UpdateCategoryDto request)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(x => x.Id == id);

        if (category == null)
        {
            return ApiResponse<CategoryDto>.FailureResponse(
                "Category not found."
            );
        }

        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return ApiResponse<CategoryDto>.FailureResponse(
                "Category name is required."
            );
        }

        var duplicate = await _context.Categories
            .AnyAsync(x =>
                x.Id != id &&
                x.Name.ToLower() == name.ToLower()
            );

        if (duplicate)
        {
            return ApiResponse<CategoryDto>.FailureResponse(
                "Another category with this name already exists."
            );
        }

        category.Name = name;

        category.Description =
            string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim();

        category.ImageUrl =
            string.IsNullOrWhiteSpace(request.ImageUrl)
                ? null
                : request.ImageUrl.Trim();

        category.IsActive = request.IsActive;

        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<CategoryDto>.SuccessResponse(
            MapToDto(category),
            "Category updated successfully."
        );
    }

    // ==========================================
    // DELETE
    // ==========================================

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(x => x.Id == id);

        if (category == null)
        {
            return ApiResponse<bool>.FailureResponse(
                "Category not found."
            );
        }

        // Soft delete
        category.IsActive = false;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(
            true,
            "Category deleted successfully."
        );
    }

    // ==========================================
    // MAPPING
    // ==========================================

    private static CategoryDto MapToDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ImageUrl = category.ImageUrl,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }
}