namespace ApplicationLayer.Contract;

public interface IChatService
{
    Task<string> GetResponseAsync(string sessionId, string userMessage, string? systemMessage = null);
}