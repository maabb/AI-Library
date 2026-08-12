using AiLibrary.Application.Abstractions;

namespace AiLibrary.Application.Dtos.Chat;

// Shared by JSON + stream handlers so chip DTOs stay consistent.
public static class ToolCallMapping
{
    public static IReadOnlyList<ToolCallDto> FromSink(IToolCallSink sink) =>
        sink.Snapshot()
            .Select(t => new ToolCallDto { Name = t.Name, Detail = t.Detail })
            .ToList();
}
