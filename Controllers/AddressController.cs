using EcommerceBackend.DTOs.Address;
using EcommerceBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceBackend.Controllers;

[ApiController]
[Route("api/addresses")]
[Authorize(Roles = "Customer")]
public class AddressController : ControllerBase
{
    private readonly IAddressService _addressService;

    public AddressController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    // ==========================================
    // GET ALL ADDRESSES
    // ==========================================

    [HttpGet]
    public async Task<IActionResult> GetMyAddresses()
    {
        var result =
            await _addressService.GetMyAddressesAsync();

        return Ok(result);
    }

    // ==========================================
    // GET ADDRESS
    // ==========================================

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAddress(Guid id)
    {
        var result =
            await _addressService.GetAddressByIdAsync(id);

        return Ok(result);
    }

    // ==========================================
    // CREATE
    // ==========================================

    [HttpPost]
    public async Task<IActionResult> CreateAddress(
        CreateAddressDto request)
    {
        var result =
            await _addressService.CreateAddressAsync(request);

        return Ok(result);
    }

    // ==========================================
    // UPDATE
    // ==========================================

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAddress(
        Guid id,
        UpdateAddressDto request)
    {
        var result =
            await _addressService.UpdateAddressAsync(
                id,
                request);

        return Ok(result);
    }

    // ==========================================
    // DELETE
    // ==========================================

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAddress(
        Guid id)
    {
        var result =
            await _addressService.DeleteAddressAsync(id);

        return Ok(result);
    }
}