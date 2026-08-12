using AiLibrary.Application.Abstractions;
using AiLibrary.Tests.Support;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace AiLibrary.Tests;

public class EfChatHistoryStoreTests : IDisposable
{
    private readonly TestSqlFixture _fx = new();

    [Fact]
    public async Task MultiTurn_SurvivesNewScope()
    {
        string sessionId;
        await using (var scope = _fx.Services.CreateAsyncScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IChatHistoryStore>();
            sessionId = Guid.NewGuid().ToString("N");
            await store.AddUserMessageAsync(sessionId, "Hello Ava");
            await store.AddAssistantMessageAsync(sessionId, "Hi there");
        }

        await using (var scope = _fx.Services.CreateAsyncScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IChatHistoryStore>();
            var history = await store.GetHistoryAsync(sessionId);
            Assert.NotEmpty(history);
            Assert.Equal(ChatRole.System, history[0].Role);
            Assert.Contains(history, m => m.Text == "Hello Ava");
            Assert.Contains(history, m => m.Text == "Hi there");
        }
    }

    [Fact]
    public async Task GetHistory_UnknownSession_IsEmpty_DoesNotCreateSession()
    {
        await using var scope = _fx.Services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IChatHistoryStore>();

        var history = await store.GetHistoryAsync(Guid.NewGuid().ToString("N"));
        Assert.Empty(history);

        var sessions = await store.ListSessionsAsync(100);
        Assert.DoesNotContain(sessions, s => history.Any());
    }

    [Fact]
    public async Task ListSessions_IncludesPreview()
    {
        await using var scope = _fx.Services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IChatHistoryStore>();
        var id = Guid.NewGuid().ToString("N");
        await store.AddUserMessageAsync(id, "Find mystery books");

        var list = await store.ListSessionsAsync(10);
        Assert.Contains(list, s => s.Id == id && s.Preview != null && s.Preview.Contains("mystery"));
    }

    [Fact]
    public async Task AddAssistant_WithoutSession_Throws()
    {
        await using var scope = _fx.Services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IChatHistoryStore>();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            store.AddAssistantMessageAsync(Guid.NewGuid().ToString("N"), "orphan reply"));
    }

    public void Dispose() => _fx.Dispose();
}
