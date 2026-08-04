using Microsoft.Extensions.AI;

namespace AiLibrary.Application.Models;

public class PromptMessage
{
    public ChatRole Role { get; set; }
    public string Content { get; set; }
}
