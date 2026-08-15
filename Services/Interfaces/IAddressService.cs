using EcommerceBackend.DTOs.Address;
using EcommerceBackend.DTOs.Common;

namespace EcommerceBackend.Services.Interfaces;

public interface IAddressService
{
    Task<ApiResponse<List<AddressDto>>> GetMyAddressesAsync();

    Task<ApiResponse<AddressDto>> GetAddressByIdAsync(
        Guid addressId);

    Task<ApiResponse<AddressDto>> CreateAddressAsync(
        CreateAddressDto request);

    Task<ApiResponse<AddressDto>> UpdateAddressAsync(
        Guid addressId,
        UpdateAddressDto request);

    Task<ApiResponse<bool>> DeleteAddressAsync(
        Guid addressId);
}