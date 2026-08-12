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
        var history = new ChatHistoryStore(new PromptBuilder());
        var handler = new StreamChatCommandHandler(chat, history);

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
}
