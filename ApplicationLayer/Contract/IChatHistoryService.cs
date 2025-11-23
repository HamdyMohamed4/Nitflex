using Microsoft.SemanticKernel.ChatCompletion;

namespace ApplicationLayer.Contract;

public interface IChatHistoryService
{
    ChatHistory GetOrCreateChatHistory(string sessionId, string? SystemMessage = null);
    void ClearHistory(string sessionId);
    bool SessionExists(string sessionId);
}