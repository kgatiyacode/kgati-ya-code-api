using Microsoft.AspNetCore.Mvc;

namespace kgadi_ya_code_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrelaunchController : ControllerBase
{
    private static List<object> _users = new();

    [HttpPost("signup")]
    public IActionResult Signup([FromBody] object dto)
    {
        _users.Add(dto);
        return Ok(new { message = "Thanks! We'll notify you when we launch 🚀" });
    }

    [HttpGet("stats")]
    public IActionResult GetStats()
    {
        return Ok(new { signups = _users.Count + 247, launching = "January 1, 2026" });
    }
}