using AiLibrary.Application.Abstractions;
using AiLibrary.Application.Models;
using Microsoft.Extensions.AI;
using ChatResponse = AiLibrary.Application.Dtos.Chat.ChatResponse;

namespace AiLibrary.Infrastructure.Services;

public class ChatService : IChatService
{
    private readonly IChatClient _chatClient;

    public ChatService(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }
    public async Task<ChatResponse> SendMessageAsync(IEnumerable<PromptMessage> prompt, CancellationToken cancellationToken)
    {
        try
        {
            var messages = prompt
                .Select(x => new ChatMessage(
                   x.Role,
                    x.Content))
                .ToList();
            
            var response = await _chatClient.GetResponseAsync(
                messages,
                new ChatOptions { MaxOutputTokens = 400 },
                cancellationToken: cancellationToken);

        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(response));
            return new ChatResponse
            {
                Response = response.Text
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }
    }
}
