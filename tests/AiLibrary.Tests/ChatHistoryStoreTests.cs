using AiLibrary.Infrastructure.Catalog;
using AiLibrary.Infrastructure.Services;
using Microsoft.Extensions.AI;

namespace AiLibrary.Tests;

public class ChatHistoryStoreTests
{
    private readonly ChatHistoryStore _store =
        new(new PromptBuilder(new InMemoryBookCatalog()));

    [Fact]
    public void AddUserMessage_SeedsSystemPromptOnce()
    {
        var snapshot = _store.AddUserMessage("s1", "Hello");

        Assert.Equal(ChatRole.System, snapshot[0].Role);
        Assert.Equal(ChatRole.User, snapshot[^1].Role);
        Assert.Equal(2, snapshot.Count);
    }

    [Fact]
    public void AddAssistantMessage_AppendsToSameSession()
    {
        _store.AddUserMessage("s1", "Q1");
        _store.AddAssistantMessage("s1", "A1");

        var history = _store.GetHistory("s1");
        Assert.Equal(3, history.Count);
        Assert.Equal("A1", history[^1].Text);
    }

    [Fact]
    public void DifferentSessions_AreIsolated()
    {
        _store.AddUserMessage("a", "from A");
        _store.AddUserMessage("b", "from B");

        var a = _store.GetHistory("a");
        var b = _store.GetHistory("b");

        Assert.Contains(a, m => m.Text == "from A");
        Assert.DoesNotContain(a, m => m.Text == "from B");
        Assert.Contains(b, m => m.Text == "from B");
        Assert.DoesNotContain(b, m => m.Text == "from A");
    }
}
