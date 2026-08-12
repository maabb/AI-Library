using System.Runtime.CompilerServices;
using AiLibrary.Application.Abstractions;
using AiLibrary.Infrastructure.Tools;
using Microsoft.Extensions.AI;

namespace AiLibrary.Infrastructure.Services;

/// <summary>
/// Thin MEAI wrapper (Learn-shaped): ChatOptions.Tools + GetResponse/Stream.
/// Does not know about Book — catalog access is via CatalogTools AIFunctions.
/// </summary>
public sealed class ChatService : IChatService
{
    private readonly IChatClient _chatClient;
    private readonly CatalogTools _catalogTools;
    private readonly IToolCallSink _toolCallSink;

    public ChatService(
        IChatClient chatClient,
        CatalogTools catalogTools,
        IToolCallSink toolCallSink)
    {
        _chatClient = chatClient;
        _catalogTools = catalogTools;
        _toolCallSink = toolCallSink;
    }

    public async Task<string> CompleteAsync(
        IEnumerable<ChatMessage> messages,
        CancellationToken cancellationToken)
    {
        // New turn: drop previous tool audit so toolsUsed matches this call only.
        _toolCallSink.Clear();

        // FunctionInvokingChatClient may run tools, then return final assistant text.
        var response = await _chatClient.GetResponseAsync(
            messages,
            CreateChatOptions(),
            cancellationToken);

        return response.Text ?? string.Empty;
    }

    public async IAsyncEnumerable<string> StreamAsync(
        IEnumerable<ChatMessage> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        _toolCallSink.Clear();

        // Stream text tokens only. Tool rounds usually finish before text starts.
        await foreach (var update in _chatClient.GetStreamingResponseAsync(
            messages,
            CreateChatOptions(),
            cancellationToken))
        {
            if (!string.IsNullOrEmpty(update.Text))
            {
                yield return update.Text;
            }
        }
    }

    // Same idea as Learn: advertise local functions on ChatOptions for this request.
    private ChatOptions CreateChatOptions() => new()
    {
        MaxOutputTokens = 800,
        Tools = _catalogTools.GetTools()
    };
}
