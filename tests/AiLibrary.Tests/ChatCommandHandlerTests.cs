using AiLibrary.Application.Commands;
using AiLibrary.Infrastructure.Services;
using AiLibrary.Tests.Fakes;
using Microsoft.Extensions.AI;

namespace AiLibrary.Tests;

public class ChatCommandHandlerTests
{
    private readonly FakeChatService _chat = new();
    private readonly ChatHistoryStore _history = new(new PromptBuilder());
    private readonly ChatCommandHandler _handler;

    public ChatCommandHandlerTests()
    {
        _handler = new ChatCommandHandler(_chat, _history);
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

        var secondPrompt = _chat.ReceivedPrompts[1];
        Assert.Contains(secondPrompt, m => m.Role == ChatRole.User && m.Text!.Contains("Hobbit"));
        Assert.Contains(secondPrompt, m => m.Role == ChatRole.Assistant && m.Text == "First answer");
        Assert.Contains(secondPrompt, m => m.Role == ChatRole.User && m.Text!.Contains("similar"));
    }

    [Fact]
    public async Task Handle_EmptyMessage_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _handler.Handle(new ChatCommand(null, "   "), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_SystemPrompt_IsGenericLibrarianPersona()
    {
        _ = await _handler.Handle(new ChatCommand(null, "Hi"), CancellationToken.None);

        var system = _chat.ReceivedPrompts[0].First(m => m.Role == ChatRole.System);
        Assert.Contains("Ava", system.Text, StringComparison.Ordinal);
        Assert.Contains("librarian", system.Text, StringComparison.OrdinalIgnoreCase);
    }
}
