using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
namespace AiLibrary.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAiServices(this IServiceCollection services, IConfiguration configuration)
    {
         services.AddSingleton<IChatClient>(_ =>
           new OllamaChatClient(configuration["Ollama:Endpoint"] 
                                   ?? throw new ArgumentNullException("Ollama:Endpoint"), 
                               configuration["Ollama:ApiKey"] 
                                   ?? throw new ArgumentNullException("Ollama:ApiKey")));
     

        // Add infrastructure services here
        return services;
    }
}
