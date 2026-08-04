using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using AiLibrary.Infrastructure.Services;
using AiLibrary.Application.Abstractions;
namespace AiLibrary.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IChatService, ChatService>();
        services.AddSingleton<IChatClient>(_ =>
           new OllamaChatClient(new Uri(configuration["Ollama:Endpoint"])
                                   , 
                               configuration["Ollama:Model"] 
                                   ));
     

        // Add infrastructure services here
        return services;
    }
}
