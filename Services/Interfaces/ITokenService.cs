using EcommerceBackend.Models;

namespace EcommerceBackend.Services.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}