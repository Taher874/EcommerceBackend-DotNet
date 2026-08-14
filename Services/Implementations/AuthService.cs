using EcommerceBackend.Data;
using EcommerceBackend.DTOs.Auth;
using EcommerceBackend.DTOs.Common;
using EcommerceBackend.Models;
using EcommerceBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;

    public AuthService(
        AppDbContext context,
        ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<ApiResponse<LoginResponse>> RegisterAsync(
        RegisterRequest request)
    {
        var email = request.Email.Trim().ToLower();

        var existingUser = await _context.Users
            .AnyAsync(x => x.Email == email);

        if (existingUser)
        {
            return ApiResponse<LoginResponse>.FailureResponse(
                "Email is already registered.");
        }

        var customerRole = await _context.Roles
            .FirstOrDefaultAsync(x => x.Name == "Customer");

        if (customerRole == null)
        {
            return ApiResponse<LoginResponse>.FailureResponse(
                "Customer role not found.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),

            FirstName = request.FirstName.Trim(),

            LastName = request.LastName.Trim(),

            Email = email,

            PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                request.Password),

            Phone = request.Phone.Trim(),

            RoleId = customerRole.Id,

            IsActive = true,

            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        var response = new LoginResponse
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = customerRole.Name
        };

        return ApiResponse<LoginResponse>.SuccessResponse(
            response,
            "Registration successful.");
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(
        LoginRequest request)
    {
        var email = request.Email.Trim().ToLower();

        var user = await _context.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Email == email);

        if (user == null)
        {
            return ApiResponse<LoginResponse>.FailureResponse(
                "Invalid email or password.");
        }

        if (!user.IsActive)
        {
            return ApiResponse<LoginResponse>.FailureResponse(
                "Your account is inactive.");
        }

        var passwordValid = BCrypt.Net.BCrypt.Verify(
            request.Password,
            user.PasswordHash);

        if (!passwordValid)
        {
            return ApiResponse<LoginResponse>.FailureResponse(
                "Invalid email or password.");
        }

        var token = _tokenService.GenerateToken(user);

        var response = new LoginResponse
        {
            Token = token,
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.Name
        };

        return ApiResponse<LoginResponse>.SuccessResponse(
            response,
            "Login successful.");
    }
}