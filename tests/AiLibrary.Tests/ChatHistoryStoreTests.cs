using AiLibrary.Infrastructure.Services;
using Microsoft.Extensions.AI;

namespace AiLibrary.Tests;

public class ChatHistoryStoreTests
{
    private readonly ChatHistoryStore _store = new(new PromptBuilder());

    [Fact]
    public async Task AddUserMessage_SeedsSystemPromptOnce()
    {
        var snapshot = await _store.AddUserMessageAsync("s1", "Hello");

        Assert.Equal(ChatRole.System, snapshot[0].Role);
        Assert.Equal(ChatRole.User, snapshot[^1].Role);
        Assert.Equal(2, snapshot.Count);
    }

    [Fact]
    public async Task AddAssistantMessage_AppendsToSameSession()
    {
        await _store.AddUserMessageAsync("s1", "Q1");
        await _store.AddAssistantMessageAsync("s1", "A1");

        var history = await _store.GetHistoryAsync("s1");
        Assert.Equal(3, history.Count);
        Assert.Equal("A1", history[^1].Text);
    }

    [Fact]
    public async Task DifferentSessions_AreIsolated()
    {
        await _store.AddUserMessageAsync("a", "from A");
        await _store.AddUserMessageAsync("b", "from B");

        var a = await _store.GetHistoryAsync("a");
        var b = await _store.GetHistoryAsync("b");

        Assert.Contains(a, m => m.Text == "from A");
        Assert.DoesNotContain(a, m => m.Text == "from B");
        Assert.Contains(b, m => m.Text == "from B");
        Assert.DoesNotContain(b, m => m.Text == "from A");
    }

    [Fact]
    public async Task ListSessions_ReturnsNewestFirst()
    {
        await _store.AddUserMessageAsync("old", "first");
        await Task.Delay(5);
        await _store.AddUserMessageAsync("new", "second");

        var list = await _store.ListSessionsAsync(10);
        Assert.True(list.Count >= 2);
        Assert.Equal("new", list[0].Id);
    }
}
