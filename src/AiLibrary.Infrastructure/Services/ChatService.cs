using System.Runtime.CompilerServices;
using AiLibrary.Application.Abstractions;
using Microsoft.Extensions.AI;

namespace AiLibrary.Infrastructure.Services;

public class ChatService : IChatService
{
    private readonly IChatClient _chatClient;

    public ChatService(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<string> CompleteAsync(
        IEnumerable<ChatMessage> messages,
        CancellationToken cancellationToken)
    {
        var response = await _chatClient.GetResponseAsync(
            messages,
            new ChatOptions { MaxOutputTokens = 800 },
            cancellationToken);

        return response.Text ?? string.Empty;
    }

    public async IAsyncEnumerable<ChatResponseUpdate> StreamAsync(
        IEnumerable<ChatMessage> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var update in _chatClient.GetStreamingResponseAsync(
            messages,
            new ChatOptions { MaxOutputTokens = 800 },
            cancellationToken))
        {
            yield return update;
        }
    }
}
