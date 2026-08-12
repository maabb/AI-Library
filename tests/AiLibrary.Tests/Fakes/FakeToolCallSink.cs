using AiLibrary.Application.Abstractions;

namespace AiLibrary.Tests.Fakes;

public sealed class FakeToolCallSink : IToolCallSink
{
    private readonly List<ToolCallInfo> _calls = [];

    public void Clear() => _calls.Clear();

    public void Record(string name, string detail) =>
        _calls.Add(new ToolCallInfo(name, detail));

    public IReadOnlyList<ToolCallInfo> Snapshot() => _calls.ToList();
}
