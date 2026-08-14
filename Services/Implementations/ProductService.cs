using EcommerceBackend.Data;
using EcommerceBackend.DTOs.Common;
using EcommerceBackend.DTOs.Product;
using EcommerceBackend.Models;
using EcommerceBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Services.Implementations;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<ProductDto>>> GetAllAsync()
    {
        var products = await _context.Products
            .AsNoTracking()
            .Where(x => x.IsActive && x.Category.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ProductDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Price = x.Price,
                DiscountPrice = x.DiscountPrice,
                StockQuantity = x.StockQuantity,
                SKU = x.SKU,
                IsActive = x.IsActive,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.Name,

                Images = x.Images
                    .Select(i => new ProductImageDto
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl
                    })
                    .ToList()
            })
            .ToListAsync();

        return ApiResponse<List<ProductDto>>.SuccessResponse(
            products,
            "Products fetched successfully."
        );
    }

    public async Task<ApiResponse<ProductDto>> GetByIdAsync(Guid id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Where(x => x.Id == id && x.IsActive)
            .Select(x => new ProductDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Price = x.Price,
                DiscountPrice = x.DiscountPrice,
                StockQuantity = x.StockQuantity,
                SKU = x.SKU,
                IsActive = x.IsActive,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.Name,

                Images = x.Images
                    .Select(i => new ProductImageDto
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (product == null)
        {
            return ApiResponse<ProductDto>.FailureResponse(
                "Product not found."
            );
        }

        return ApiResponse<ProductDto>.SuccessResponse(
            product,
            "Product fetched successfully."
        );
    }

    public async Task<ApiResponse<ProductDto>> CreateAsync(
        CreateProductDto request)
    {
        var name = request.Name.Trim();
        var sku = request.SKU.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return ApiResponse<ProductDto>.FailureResponse(
                "Product name is required."
            );
        }

        if (string.IsNullOrWhiteSpace(sku))
        {
            return ApiResponse<ProductDto>.FailureResponse(
                "SKU is required."
            );
        }

        var categoryExists = await _context.Categories
            .AnyAsync(x =>
                x.Id == request.CategoryId &&
                x.IsActive);

        if (!categoryExists)
        {
            return ApiResponse<ProductDto>.FailureResponse(
                "Category not found."
            );
        }

        var skuExists = await _context.Products
            .AnyAsync(x =>
                x.SKU.ToLower() == sku.ToLower());

        if (skuExists)
        {
            return ApiResponse<ProductDto>.FailureResponse(
                "A product with this SKU already exists."
            );
        }

        if (request.DiscountPrice.HasValue &&
            request.DiscountPrice.Value >= request.Price)
        {
            return ApiResponse<ProductDto>.FailureResponse(
                "Discount price must be less than the original price."
            );
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),

            Name = name,

            Description = string.IsNullOrWhiteSpace(
                request.Description)
                ? null
                : request.Description.Trim(),

            Price = request.Price,

            DiscountPrice = request.DiscountPrice,

            StockQuantity = request.StockQuantity,

            SKU = sku,

            CategoryId = request.CategoryId,

            IsActive = true,

            CreatedAt = DateTime.UtcNow
        };

        foreach (var imageUrl in request.ImageUrls)
        {
            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                product.Images.Add(new ProductImage
                {
                    Id = Guid.NewGuid(),
                    ImageUrl = imageUrl.Trim()
                });
            }
        }

        _context.Products.Add(product);

        await _context.SaveChangesAsync();

        var result = await GetProductDto(product.Id);

        return ApiResponse<ProductDto>.SuccessResponse(
            result!,
            "Product created successfully."
        );
    }

    public async Task<ApiResponse<ProductDto>> UpdateAsync(
        Guid id,
        UpdateProductDto request)
    {
        var product = await _context.Products
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (product == null)
        {
            return ApiResponse<ProductDto>.FailureResponse(
                "Product not found."
            );
        }

        var categoryExists = await _context.Categories
            .AnyAsync(x =>
                x.Id == request.CategoryId &&
                x.IsActive);

        if (!categoryExists)
        {
            return ApiResponse<ProductDto>.FailureResponse(
                "Category not found."
            );
        }

        var skuExists = await _context.Products
            .AnyAsync(x =>
                x.Id != id &&
                x.SKU.ToLower() == request.SKU.Trim().ToLower());

        if (skuExists)
        {
            return ApiResponse<ProductDto>.FailureResponse(
                "A product with this SKU already exists."
            );
        }

        if (request.DiscountPrice.HasValue &&
            request.DiscountPrice.Value >= request.Price)
        {
            return ApiResponse<ProductDto>.FailureResponse(
                "Discount price must be less than the original price."
            );
        }

        product.Name = request.Name.Trim();
        product.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();

        product.Price = request.Price;
        product.DiscountPrice = request.DiscountPrice;
        product.StockQuantity = request.StockQuantity;
        product.SKU = request.SKU.Trim();
        product.CategoryId = request.CategoryId;
        product.IsActive = request.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        _context.ProductImages.RemoveRange(product.Images);

        foreach (var imageUrl in request.ImageUrls)
        {
            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                product.Images.Add(new ProductImage
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    ImageUrl = imageUrl.Trim()
                });
            }
        }

        await _context.SaveChangesAsync();

        var result = await GetProductDto(product.Id);

        return ApiResponse<ProductDto>.SuccessResponse(
            result!,
            "Product updated successfully."
        );
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(x => x.Id == id);

        if (product == null)
        {
            return ApiResponse<bool>.FailureResponse(
                "Product not found."
            );
        }

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(
            true,
            "Product deleted successfully."
        );
    }

    private async Task<ProductDto?> GetProductDto(Guid id)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ProductDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Price = x.Price,
                DiscountPrice = x.DiscountPrice,
                StockQuantity = x.StockQuantity,
                SKU = x.SKU,
                IsActive = x.IsActive,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.Name,

                Images = x.Images
                    .Select(i => new ProductImageDto
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }
}