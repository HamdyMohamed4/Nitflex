using System.ComponentModel.DataAnnotations;

namespace Presentation.Models;

public class ChatRequest
{
    [Required]
    public string SessionId { get; set; } = string.Empty;
    [Required]
    [MinLength(1, ErrorMessage = "UserMessage should be atleast 1 character.")]
    public string UserMessage { get; set; } = string.Empty;
    public string? SystemMessage { get; set; }
}