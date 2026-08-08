using Microsoft.Extensions.AI;

namespace AiLibrary.Application.Abstractions;

public interface IChatService
{
    Task<Dtos.Chat.ChatResponse> SendMessageAsync(
        IEnumerable<ChatMessage> prompt,
        CancellationToken cancellationToken);
}