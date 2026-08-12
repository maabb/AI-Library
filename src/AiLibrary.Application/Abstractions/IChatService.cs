using Microsoft.Extensions.AI;

namespace AiLibrary.Application.Abstractions;

public interface IChatService
{
    Task<string> CompleteAsync(
        IEnumerable<ChatMessage> messages,
        CancellationToken cancellationToken);

    IAsyncEnumerable<ChatResponseUpdate> StreamAsync(
        IEnumerable<ChatMessage> messages,
        CancellationToken cancellationToken);
}
