using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceBackend.Controllers;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    [HttpGet("public")]
    public IActionResult Public()
    {
        return Ok("Anyone can access this.");
    }

    [Authorize]
    [HttpGet("private")]
    [ApiExplorerSettings(GroupName = "frontend")]
    public IActionResult Private()
    {
        return Ok("You are authenticated.");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    [ApiExplorerSettings(GroupName = "admin")]
    public IActionResult Admin()
    {
        return Ok("You are an admin.");
    }
}