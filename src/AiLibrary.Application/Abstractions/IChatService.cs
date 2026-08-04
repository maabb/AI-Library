using AiLibrary.Application.Dtos.Chat;
using AiLibrary.Application.Models;

namespace AiLibrary.Application.Abstractions;

public interface IChatService
{
    Task<ChatResponse> SendMessageAsync(
       IEnumerable<PromptMessage> prompt,
       CancellationToken cancellationToken);
}
