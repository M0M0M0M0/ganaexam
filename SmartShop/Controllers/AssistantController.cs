using Microsoft.AspNetCore.Mvc;
using SmartShop.Dtos;
using SmartShop.Services;

namespace SmartShop.Controllers;

[ApiController]
[Route("api/assistant")]
public class AssistantController(ShoppingAssistantService assistant, ChatHistoryStore historyStore) : ControllerBase
{
    [HttpPost("chat", Name = "shoppingassistant")]
    [ProducesResponseType<ChatResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request, CancellationToken ct) =>
        Ok(await assistant.ChatAsync(request, ct));

    [HttpGet("history/{sessionId}")]
    public IActionResult GetHistory(string sessionId) =>
        Ok(historyStore.GetHistory(sessionId).Select(m => new ChatHistoryItem(m.Role, m.Text)));

    [HttpDelete("history/{sessionId}")]
    public IActionResult ClearHistory(string sessionId)
    {
        historyStore.Clear(sessionId);
        return NoContent();
    }
}
