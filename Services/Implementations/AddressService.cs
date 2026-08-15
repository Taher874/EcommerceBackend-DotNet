using System.Security.Claims;
using EcommerceBackend.Data;
using EcommerceBackend.DTOs.Address;
using EcommerceBackend.DTOs.Common;
using EcommerceBackend.Models;
using EcommerceBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Services.Implementations;

public class AddressService : IAddressService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AddressService(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    private Guid GetUserId()
    {
        var userId = _httpContextAccessor.HttpContext?
            .User
            .FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userId, out var id))
        {
            throw new UnauthorizedAccessException(
                "User is not authenticated.");
        }

        return id;
    }

    // ==========================================
    // GET MY ADDRESSES
    // ==========================================

    public async Task<ApiResponse<List<AddressDto>>>
        GetMyAddressesAsync()
    {
        var userId = GetUserId();

        var addresses = await _context.Addresses
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.Id)
            .ToListAsync();

        var result = addresses
            .Select(MapAddress)
            .ToList();

        return ApiResponse<List<AddressDto>>.SuccessResponse(
            result,
            "Addresses fetched successfully.");
    }

    // ==========================================
    // GET ADDRESS
    // ==========================================

    public async Task<ApiResponse<AddressDto>>
        GetAddressByIdAsync(Guid addressId)
    {
        var userId = GetUserId();

        var address = await _context.Addresses
            .FirstOrDefaultAsync(x =>
                x.Id == addressId &&
                x.UserId == userId);

        if (address == null)
        {
            return ApiResponse<AddressDto>.FailureResponse(
                "Address not found.");
        }

        return ApiResponse<AddressDto>.SuccessResponse(
            MapAddress(address),
            "Address fetched successfully.");
    }

    // ==========================================
    // CREATE ADDRESS
    // ==========================================

    public async Task<ApiResponse<AddressDto>>
        CreateAddressAsync(CreateAddressDto request)
    {
        var userId = GetUserId();

        var address = new Address
        {
            Id = Guid.NewGuid(),

            UserId = userId,

            FullName = request.FullName.Trim(),

            Phone = request.Phone.Trim(),

            AddressLine1 = request.AddressLine1.Trim(),

            AddressLine2 =
                string.IsNullOrWhiteSpace(request.AddressLine2)
                    ? null
                    : request.AddressLine2.Trim(),

            City = request.City.Trim(),

            State = request.State.Trim(),

            PostalCode = request.PostalCode.Trim(),

            Country = request.Country.Trim()
        };

        _context.Addresses.Add(address);

        await _context.SaveChangesAsync();

        return ApiResponse<AddressDto>.SuccessResponse(
            MapAddress(address),
            "Address created successfully.");
    }

    // ==========================================
    // UPDATE ADDRESS
    // ==========================================

    public async Task<ApiResponse<AddressDto>>
        UpdateAddressAsync(
            Guid addressId,
            UpdateAddressDto request)
    {
        var userId = GetUserId();

        var address = await _context.Addresses
            .FirstOrDefaultAsync(x =>
                x.Id == addressId &&
                x.UserId == userId);

        if (address == null)
        {
            return ApiResponse<AddressDto>.FailureResponse(
                "Address not found.");
        }

        address.FullName = request.FullName.Trim();

        address.Phone = request.Phone.Trim();

        address.AddressLine1 =
            request.AddressLine1.Trim();

        address.AddressLine2 =
            string.IsNullOrWhiteSpace(request.AddressLine2)
                ? null
                : request.AddressLine2.Trim();

        address.City = request.City.Trim();

        address.State = request.State.Trim();

        address.PostalCode =
            request.PostalCode.Trim();

        address.Country =
            request.Country.Trim();

        await _context.SaveChangesAsync();

        return ApiResponse<AddressDto>.SuccessResponse(
            MapAddress(address),
            "Address updated successfully.");
    }

    // ==========================================
    // DELETE ADDRESS
    // ==========================================

    public async Task<ApiResponse<bool>>
        DeleteAddressAsync(Guid addressId)
    {
        var userId = GetUserId();

        var address = await _context.Addresses
            .FirstOrDefaultAsync(x =>
                x.Id == addressId &&
                x.UserId == userId);

        if (address == null)
        {
            return ApiResponse<bool>.FailureResponse(
                "Address not found.");
        }

        _context.Addresses.Remove(address);

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(
            true,
            "Address deleted successfully.");
    }

    // ==========================================
    // MAP
    // ==========================================

    private static AddressDto MapAddress(Address address)
    {
        return new AddressDto
        {
            Id = address.Id,

            FullName = address.FullName,

            Phone = address.Phone,

            AddressLine1 = address.AddressLine1,

            AddressLine2 = address.AddressLine2,

            City = address.City,

            State = address.State,

            PostalCode = address.PostalCode,

            Country = address.Country
        };
    }
}