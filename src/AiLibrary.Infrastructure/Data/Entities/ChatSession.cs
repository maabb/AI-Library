namespace AiLibrary.Infrastructure.Data.Entities;

// One conversation thread. Id == client sessionId (survives API restart).
public sealed class ChatSession
{
    public string Id { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    // Used to sort the sidebar (newest first).
    public DateTimeOffset UpdatedAt { get; set; }
    public List<ChatMessageRow> Messages { get; set; } = [];
}
