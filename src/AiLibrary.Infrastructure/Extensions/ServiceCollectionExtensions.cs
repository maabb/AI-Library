using AiLibrary.Application.Abstractions;
using AiLibrary.Infrastructure.Data;
using AiLibrary.Infrastructure.Data.Repositories;
using AiLibrary.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AiLibrary.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var endpoint = configuration["Ollama:Endpoint"]
            ?? throw new InvalidOperationException(
                "Missing config 'Ollama:Endpoint'. Example: http://localhost:11434");

        var model = configuration["Ollama:Model"]
            ?? throw new InvalidOperationException(
                "Missing config 'Ollama:Model'. Example: gemma3:4b");

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri))
        {
            throw new InvalidOperationException(
                $"Invalid Ollama endpoint URI: '{endpoint}'");
        }

        var connectionString = configuration.GetConnectionString("Default")
            ?? "Data Source=ailibrary";

        services.AddDbContextFactory<SqlContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped(typeof(ISqlRepository<>), typeof(SqlRepository<>));
        services.AddSingleton<IPromptBuilder, PromptBuilder>();
        services.AddSingleton<IChatHistoryStore, ChatHistoryStore>();
        services.AddScoped<IChatService, ChatService>();

        services.AddSingleton<IChatClient>(_ =>
            new OllamaChatClient(endpointUri, model));

        return services;
    }

    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        var factory = services.GetRequiredService<IDbContextFactory<SqlContext>>();
        await using var db = await factory.CreateDbContextAsync();
        await db.Database.MigrateAsync();
    }
}
