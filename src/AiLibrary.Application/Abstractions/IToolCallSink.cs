namespace AiLibrary.Application.Abstractions;

public sealed record ToolCallInfo(string Name, string Detail);

// Product feature (not MEAI): handlers read this after the model call for toolsUsed/chips.
public interface IToolCallSink
{
    void Clear();
    void Record(string name, string detail);
    IReadOnlyList<ToolCallInfo> Snapshot();
}
