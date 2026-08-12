namespace AiLibrary.Application.Dtos.Chat;

// Sidebar row (id + when + short preview). Not a MEAI type.
public sealed class ChatSessionInfo
{
    public required string Id { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public string? Preview { get; init; }
}
