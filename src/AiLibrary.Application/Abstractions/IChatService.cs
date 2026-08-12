using Microsoft.Extensions.AI;

namespace AiLibrary.Application.Abstractions;

// Application port for the LLM. Tool registration lives in Infrastructure (ChatOptions).
public interface IChatService
{
    Task<string> CompleteAsync(
        IEnumerable<ChatMessage> messages,
        CancellationToken cancellationToken);

    // Text tokens only; toolsUsed is attached by the command handler after the stream.
    IAsyncEnumerable<string> StreamAsync(
        IEnumerable<ChatMessage> messages,
        CancellationToken cancellationToken);
}
