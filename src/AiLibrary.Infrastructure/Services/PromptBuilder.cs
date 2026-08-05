using AiLibrary.Application.Abstractions;
using Microsoft.Extensions.AI;

namespace AiLibrary.Infrastructure.Services;

public class PromptBuilder : IPromptBuilder
{
    public IEnumerable<ChatMessage> BuildPrompt(ChatMessage context)
    {
        return
        [
            new(ChatRole.System, "You are a helpful AI assistant."),
            context
        ];
    }
}