using ApplicationLayer.Contract;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IChatHistoryService _chatHistoryService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IChatService chatService, IChatHistoryService chatHistoryService, ILogger<ChatController> logger)
    {
        _chatService = chatService;
        _chatHistoryService = chatHistoryService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Chat([FromBody] ChatRequest chatRequest)
    {
        try
        {
            string response = await _chatService.GetResponseAsync(sessionId: chatRequest.SessionId, userMessage: chatRequest.UserMessage, systemMessage: chatRequest.SystemMessage);

            return Ok(new { Status = true, ChatBotResponse = response });
        }
        catch (Exception ex)
        {
            _logger.LogError($"An Error Occured with Message: {ex.Message}");
            return BadRequest(new { Status = false, Message = $"An Error Occured with Message: {ex.Message}" });
        }
    }

    [HttpPost]
    [Route("ClearChat/{sessionId}")]
    public async Task<IActionResult> ClearChat(string sessionId)
    {
        _chatHistoryService.ClearHistory(sessionId);
        return Ok(new { Status = true, Message = $"Cleared Chat History with SessionId: {sessionId}" });
    }

    [HttpGet("SessionExists/{sessionId}")]
    public async Task<IActionResult> SessionExists(string sessionId)
    {
        var response = _chatHistoryService.SessionExists(sessionId);

        return Ok(new { Status = true, SessionExists = response });
    }
}