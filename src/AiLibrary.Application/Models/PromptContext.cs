using Microsoft.Extensions.AI;

namespace AiLibrary.Application.Models;

public class PromptContext
{
    public string UserMessage { get; set; }

    public string SystemInstructions { get; set; }

    public string UserName { get; set; }

    public IReadOnlyList<ChatMessage> ConversationHistory { get; set; }

    public IReadOnlyList<string> RetrievedDocuments { get; set; }
}
