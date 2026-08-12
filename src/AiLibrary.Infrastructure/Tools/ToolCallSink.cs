using System.Collections.Concurrent;
using AiLibrary.Application.Abstractions;

namespace AiLibrary.Infrastructure.Tools;

// Scoped per HTTP request: which tools ran this turn (for toolsUsed / UI chips).
public sealed class ToolCallSink : IToolCallSink
{
    private readonly ConcurrentQueue<ToolCallInfo> _calls = new();

    public void Clear()
    {
        while (_calls.TryDequeue(out _))
        {
        }
    }

    public void Record(string name, string detail) =>
        _calls.Enqueue(new ToolCallInfo(name, detail));

    public IReadOnlyList<ToolCallInfo> Snapshot() => _calls.ToArray();
}
