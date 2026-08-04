using AiLibrary.Application.Models;

namespace AiLibrary.Application.Abstractions;

public interface IPromptBuilder
{
    IEnumerable <PromptMessage> BuildPrompt (PromptContext context);
}
