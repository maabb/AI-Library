namespace AiLibrary.Application.Dtos.Chat;

public class ChatResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    // Empty when the model answered without calling a tool.
    public IReadOnlyList<ToolCallDto> ToolsUsed { get; set; } = [];
}

public sealed class ToolCallDto
{
    public required string Name { get; init; }
    public required string Detail { get; init; }
}
