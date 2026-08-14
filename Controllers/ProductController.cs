using EcommerceBackend.DTOs.Product;
using EcommerceBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceBackend.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _productService.GetAllAsync();

        return Ok(response);
    }


    [HttpGet("category/{categoryId:guid}")]
    public async Task<IActionResult> GetByCategory(Guid categoryId)
    {
        var response =
            await _productService.GetByCategoryAsync(categoryId);

        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _productService.GetByIdAsync(id);

        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }

    [Authorize(Roles = "Admin")]
    [ApiExplorerSettings(GroupName = "admin")]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductDto request)
    {
        var response = await _productService.CreateAsync(request);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [Authorize(Roles = "Admin")]
    [ApiExplorerSettings(GroupName = "admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateProductDto request)
    {
        var response =
            await _productService.UpdateAsync(id, request);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [Authorize(Roles = "Admin")]
    [ApiExplorerSettings(GroupName = "admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var response = await _productService.DeleteAsync(id);

        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }
}