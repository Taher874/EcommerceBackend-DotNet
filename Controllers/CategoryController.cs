using EcommerceBackend.DTOs.Category;
using EcommerceBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceBackend.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(
        ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // ==========================================
    // GET ALL CATEGORIES
    // Public
    // ==========================================

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response =
            await _categoryService.GetAllAsync();

        return Ok(response);
    }

 

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response =
            await _categoryService.GetByIdAsync(id);

        if (!response.Success)
        {
            return NotFound(response);
        }

        return Ok(response);
    }

    // ==========================================
    // CREATE CATEGORY
    // Admin Only
    // ==========================================

    [Authorize(Roles = "Admin")]
    [ApiExplorerSettings(GroupName = "admin")]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCategoryDto request)
    {
        var response =
            await _categoryService.CreateAsync(request);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    // ==========================================
    // UPDATE CATEGORY
    // Admin Only
    // ==========================================

    [Authorize(Roles = "Admin")]
    [ApiExplorerSettings(GroupName = "admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateCategoryDto request)
    {
        var response =
            await _categoryService.UpdateAsync(
                id,
                request
            );

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    // ==========================================
    // DELETE CATEGORY
    // Admin Only
    // ==========================================

    [Authorize(Roles = "Admin")]
    [ApiExplorerSettings(GroupName = "admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var response =
            await _categoryService.DeleteAsync(id);

        if (!response.Success)
        {
            return NotFound(response);
        }

        return Ok(response);
    }
}