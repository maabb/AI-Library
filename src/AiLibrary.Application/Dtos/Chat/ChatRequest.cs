using System.ComponentModel.DataAnnotations;

namespace AiLibrary.Application.Dtos.Chat;

public class ChatRequest
{
    /// <summary>
    /// Stable id for multi-turn chat. Omit or leave empty to start a new session.
    /// </summary>
    public string? SessionId { get; set; }

    [Required]
    [MinLength(1)]
    public string Message { get; set; } = string.Empty;
}
