using Microsoft.AspNetCore.Mvc;

namespace kgadi_ya_code_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (request.Email == "test@example.com" && request.Password == "password")
        {
            return Ok(new { 
                jwt = "mock_jwt_token_" + DateTime.Now.Ticks,
                refreshToken = "mock_refresh_token_" + DateTime.Now.Ticks
            });
        }
        return Unauthorized();
    }

    [HttpPost("refresh")]
    public IActionResult Refresh([FromBody] object request)
    {
        return Ok(new { 
            jwt = "refreshed_jwt_token_" + DateTime.Now.Ticks,
            refreshToken = "refreshed_refresh_token_" + DateTime.Now.Ticks
        });
    }
}

public class LoginRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}