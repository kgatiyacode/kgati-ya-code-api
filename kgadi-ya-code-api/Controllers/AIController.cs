using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using kgadi_ya_code_api.Services;

namespace kgadi_ya_code_api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AIController : ControllerBase
{
    private readonly IAIService _aiService;

    public AIController(IAIService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        try
        {
            var response = await _aiService.ChatWithLLMAsync(request.Message, request.Context);
            return Ok(new { response, timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Chat failed", error = ex.Message });
        }
    }

    [HttpPost("simplify")]
    public async Task<IActionResult> SimplifyText([FromBody] SimplifyRequest request)
    {
        try
        {
            var simplified = await _aiService.SimplifyTextAsync(request.Text, request.UserLevel);
            return Ok(new { originalText = request.Text, simplifiedText = simplified });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Simplification failed", error = ex.Message });
        }
    }

    [HttpGet("training-data/{industry}/{businessType}")]
    public async Task<IActionResult> GetTrainingData(string industry, string businessType)
    {
        try
        {
            var suggestions = await _aiService.GetTrainingDataSuggestionsAsync(industry, businessType);
            return Ok(new { industry, businessType, dataSources = suggestions });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Training data suggestions failed", error = ex.Message });
        }
    }
}

public class ChatRequest
{
    public string Message { get; set; } = "";
    public string Context { get; set; } = "";
}

public class SimplifyRequest
{
    public string Text { get; set; } = "";
    public string UserLevel { get; set; } = "beginner"; // beginner, intermediate, advanced
}