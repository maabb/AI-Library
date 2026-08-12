using System.Text;
using AiLibrary.Application.Commands;
using AiLibrary.Infrastructure.Services;
using AiLibrary.Tests.Fakes;
using Microsoft.Extensions.AI;

namespace AiLibrary.Tests;

public class StreamChatCommandHandlerTests
{
    [Fact]
    public async Task Stream_YieldsTokens_AndPersistsFullAssistantMessage()
    {
        var chat = new FakeChatService { NextReply = "ABC" };
        var sink = new FakeToolCallSink();
        var history = new ChatHistoryStore(new PromptBuilder());
        var handler = new StreamChatCommandHandler(chat, history, sink);

        var result = await handler.Handle(
            new StreamChatCommand(null, "stream please"),
            CancellationToken.None);

        var sb = new StringBuilder();
        await foreach (var token in result.Tokens)
        {
            sb.Append(token);
        }

        Assert.Equal("ABC", sb.ToString());
        var stored = history.GetHistory(result.SessionId);
        Assert.Contains(stored, m => m.Role == ChatRole.Assistant && m.Text == "ABC");
    }

    [Fact]
    public async Task Stream_ExposesToolsUsed_AfterTokensComplete()
    {
        var sink = new FakeToolCallSink();
        var chat = new RecordingStreamChat(sink) { NextReply = "OK" };
        var history = new ChatHistoryStore(new PromptBuilder());
        var handler = new StreamChatCommandHandler(chat, history, sink);

        var result = await handler.Handle(
            new StreamChatCommand(null, "fantasy"),
            CancellationToken.None);

        await foreach (var _ in result.Tokens)
        {
        }

        Assert.Contains(result.ToolsUsed, t => t.Name == "search_catalog");
    }

    private sealed class RecordingStreamChat : AiLibrary.Application.Abstractions.IChatService
    {
        private readonly FakeToolCallSink _sink;

        public string NextReply { get; set; } = "";

        public RecordingStreamChat(FakeToolCallSink sink) => _sink = sink;

        public Task<string> CompleteAsync(
            IEnumerable<ChatMessage> messages,
            CancellationToken cancellationToken) =>
            Task.FromResult(NextReply);

        public async IAsyncEnumerable<string> StreamAsync(
            IEnumerable<ChatMessage> messages,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            _sink.Clear();
            _sink.Record("search_catalog", "query=fantasy");
            yield return NextReply;
            await Task.Yield();
        }
    }
}
