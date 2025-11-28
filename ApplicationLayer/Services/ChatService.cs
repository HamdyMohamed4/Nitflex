using ApplicationLayer.Contract;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace ApplicationLayer.Services;

public class ChatService : IChatService
{
    private readonly Kernel _kernel;
    private readonly ILogger<ChatService> _logger;
    private readonly IChatHistoryService _chatHistoryService;
    private const string DefaultSystemMessage = "you are a friendly chatbot. people are going to ask you questions about Netflix series and movies, give them accurate, brief answers that do not exceed 10 words";

    public ChatService(Kernel kernel, ILogger<ChatService> logger, IChatHistoryService chatHistoryService)
    {
        _kernel = kernel;
        _logger = logger;
        _chatHistoryService = chatHistoryService;
    }

    public async Task<string> GetResponseAsync(string sessionId, string userMessage, string? systemMessage = null)
    {
        ChatHistory history = _chatHistoryService.GetOrCreateChatHistory(sessionId, systemMessage ?? DefaultSystemMessage);

        history.AddUserMessage(userMessage);

        _logger.LogInformation($"Added User Message: {userMessage} for chat with sessionId: {sessionId}");

        var chatService = _kernel.GetRequiredService<IChatCompletionService>();

        ChatMessageContent response = await chatService.GetChatMessageContentAsync(history);

        string responseText = response.Content ?? "I'm not able to give you a presice answer.";

        history.AddAssistantMessage(responseText);

        _logger.LogInformation($"SessionId: {sessionId}, AssistantMessage: {responseText}");

        return responseText;
    }
}