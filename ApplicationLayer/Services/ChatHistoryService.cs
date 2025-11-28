using System.Collections.Concurrent;
using ApplicationLayer.Contract;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;

namespace ApplicationLayer.Services;

public class ChatHistoryService : IChatHistoryService
{
    private readonly ConcurrentDictionary<string, ChatHistory> _histories = new();
    private readonly ILogger<ChatHistoryService> _logger;

    public ChatHistoryService(ILogger<ChatHistoryService> logger)
    {
        _logger = logger;
    }

    public ChatHistory GetOrCreateChatHistory(string sessionId, string? SystemMessage = null)
    {
        return _histories.GetOrAdd(sessionId, id =>
        {
            _logger.LogInformation($"Creating a new Chat History of session with Id: {sessionId}");
            return string.IsNullOrEmpty(SystemMessage) ? new ChatHistory() : new ChatHistory(SystemMessage);
        });
    }

    public void ClearHistory(string sessionId)
    {
        if (_histories.TryRemove(sessionId, out _))
            _logger.LogInformation($"Chat History with SessionId: {sessionId} is cleared successfully.");
    }

    public bool SessionExists(string sessionId)
    {
        return _histories.ContainsKey(sessionId);
    }
}