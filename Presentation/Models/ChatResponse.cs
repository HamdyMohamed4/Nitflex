using System.ComponentModel.DataAnnotations;

namespace Presentation.Models;

public class ChatResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string ResponseText { get; set; } = string.Empty;
    public DateTime TimeSpan { get; set; } = DateTime.Now;
}