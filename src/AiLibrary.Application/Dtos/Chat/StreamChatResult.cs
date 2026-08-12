namespace AiLibrary.Application.Dtos.Chat;

// Stream use-case result. ToolsUsed is set after Tokens finishes enumerating.
public sealed class StreamChatResult
{
    public required string SessionId { get; init; }
    public required IAsyncEnumerable<string> Tokens { get; init; }
    public IReadOnlyList<ToolCallDto> ToolsUsed { get; set; } = [];
}
