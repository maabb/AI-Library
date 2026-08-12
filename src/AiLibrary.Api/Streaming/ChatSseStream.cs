using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Text.Json;
using AiLibrary.Application.Dtos.Chat;

namespace AiLibrary.Api.Streaming;

/// <summary>
/// Maps StreamChatResult → BCL SseItem for TypedResults.ServerSentEvents.
/// Keeps event names/payloads stable for the Angular client.
/// </summary>
public static class ChatSseStream
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async IAsyncEnumerable<SseItem<string>> ToSseItems(
        StreamChatResult result,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        yield return Item("session", new { sessionId = result.SessionId });

        await foreach (var token in result.Tokens.WithCancellation(cancellationToken))
        {
            yield return Item("token", new { text = token });
        }

        // ToolsUsed is populated only after Tokens completes (see StreamChatCommandHandler).
        yield return Item("done", new { sessionId = result.SessionId, toolsUsed = result.ToolsUsed });
    }

    private static SseItem<string> Item(string eventType, object payload) =>
        new(JsonSerializer.Serialize(payload, JsonOptions), eventType);
}
