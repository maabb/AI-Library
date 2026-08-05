using AiLibrary.Application.Abstractions;
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
    public async Task<ChatResponse> SendMessageAsync(IEnumerable<ChatMessage> prompt, CancellationToken cancellationToken)
    {
        try
        {
          
            
            var response = await _chatClient.GetResponseAsync(
                prompt,
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
