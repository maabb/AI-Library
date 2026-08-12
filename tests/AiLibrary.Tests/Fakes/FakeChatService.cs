using AiLibrary.Application.Abstractions;
using Microsoft.Extensions.AI;

namespace AiLibrary.Tests.Fakes;

public sealed class FakeChatService : IChatService
{
    public string NextReply { get; set; } = "Fake librarian reply.";
    public List<IReadOnlyList<ChatMessage>> ReceivedPrompts { get; } = [];

    public Task<string> CompleteAsync(
        IEnumerable<ChatMessage> messages,
        CancellationToken cancellationToken)
    {
        ReceivedPrompts.Add(messages.ToList());
        return Task.FromResult(NextReply);
    }

    public async IAsyncEnumerable<string> StreamAsync(
        IEnumerable<ChatMessage> messages,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ReceivedPrompts.Add(messages.ToList());
        foreach (var ch in NextReply)
        {
            yield return ch.ToString();
            await Task.Yield();
        }
    }
}
