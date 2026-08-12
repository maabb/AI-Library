using AiLibrary.Application.Abstractions;
using AiLibrary.Application.Commands;
using AiLibrary.Infrastructure.Services;
using AiLibrary.Tests.Fakes;
using Microsoft.Extensions.AI;

namespace AiLibrary.Tests;

public class ChatCommandHandlerTests
{
    private readonly FakeChatService _chat = new();
    private readonly FakeToolCallSink _tools = new();
    private readonly ChatHistoryStore _history = new(new PromptBuilder());
    private readonly ChatCommandHandler _handler;

    public ChatCommandHandlerTests()
    {
        _handler = new ChatCommandHandler(_chat, _history, _tools);
    }

    [Fact]
    public async Task Handle_CreatesSession_AndReturnsAssistantReply()
    {
        _chat.NextReply = "Welcome to the library.";

        var result = await _handler.Handle(
            new ChatCommand(null, "Hello"),
            CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(result.SessionId));
        Assert.Equal("Welcome to the library.", result.Message);
        Assert.Single(_chat.ReceivedPrompts);
        Assert.Contains(_chat.ReceivedPrompts[0], m => m.Role == ChatRole.System);
        Assert.Contains(_chat.ReceivedPrompts[0], m => m.Role == ChatRole.User && m.Text == "Hello");
    }

    [Fact]
    public async Task Handle_ReusesSession_AndKeepsPriorTurns()
    {
        _chat.NextReply = "First answer";
        var first = await _handler.Handle(
            new ChatCommand(null, "Tell me about The Hobbit"),
            CancellationToken.None);

        _chat.NextReply = "Second answer about similar books";
        var second = await _handler.Handle(
            new ChatCommand(first.SessionId, "Suggest similar books"),
            CancellationToken.None);

        Assert.Equal(first.SessionId, second.SessionId);
        Assert.Equal(2, _chat.ReceivedPrompts.Count);
    }

    [Fact]
    public async Task Handle_EmptyMessage_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _handler.Handle(new ChatCommand(null, "   "), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_SystemPrompt_MentionsTools()
    {
        _ = await _handler.Handle(new ChatCommand(null, "Hi"), CancellationToken.None);

        var system = _chat.ReceivedPrompts[0].First(m => m.Role == ChatRole.System);
        Assert.Contains("Ava", system.Text, StringComparison.Ordinal);
        Assert.Contains("search_catalog", system.Text, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Handle_IncludesToolsUsed_FromSink()
    {
        var chat = new RecordingChatService(_tools) { NextReply = "Try Orient Express." };
        var handler = new ChatCommandHandler(chat, _history, _tools);

        var result = await handler.Handle(new ChatCommand(null, "mystery please"), CancellationToken.None);

        Assert.Contains(result.ToolsUsed, t => t.Name == "search_catalog");
        Assert.Equal("Try Orient Express.", result.Message);
    }

    private sealed class RecordingChatService : IChatService
    {
        private readonly IToolCallSink _sink;

        public string NextReply { get; set; } = "";

        public RecordingChatService(IToolCallSink sink) => _sink = sink;

        public Task<string> CompleteAsync(
            IEnumerable<ChatMessage> messages,
            CancellationToken cancellationToken)
        {
            _sink.Clear();
            _sink.Record("search_catalog", "query=mystery");
            return Task.FromResult(NextReply);
        }

        public async IAsyncEnumerable<string> StreamAsync(
            IEnumerable<ChatMessage> messages,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            yield return NextReply;
            await Task.Yield();
        }
    }
}
