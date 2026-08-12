namespace AiLibrary.Infrastructure.Data.Entities;

// One stored turn. Not MEAI ChatMessage — EF row only; mapped when loading for the model.
public sealed class ChatMessageRow
{
    public Guid Id { get; set; }
    public string SessionId { get; set; } = null!;
    // MEAI role name: system | user | assistant
    public string Role { get; set; } = null!;
    public string Content { get; set; } = null!;
    // Stable order within the session (0 = system seed).
    public int Sequence { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public ChatSession Session { get; set; } = null!;
}
