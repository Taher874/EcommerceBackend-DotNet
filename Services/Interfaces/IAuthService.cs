using EcommerceBackend.DTOs.Auth;
using EcommerceBackend.DTOs.Common;

namespace EcommerceBackend.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<LoginResponse>> RegisterAsync(
        RegisterRequest request);

    Task<ApiResponse<LoginResponse>> LoginAsync(
        LoginRequest request);
}