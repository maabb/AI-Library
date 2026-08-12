using AiLibrary.Application.Abstractions;
using AiLibrary.Infrastructure.Data;
using AiLibrary.Infrastructure.Data.Repositories;
using AiLibrary.Infrastructure.Services;
using AiLibrary.Infrastructure.Tools;
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
            ?? "Data Source=ailibrary.db";

        // Local SQLite file (see ConnectionStrings:Default). No separate server process.
        // Scoped DbContext = one context per HTTP request (injected into EfChatHistoryStore).
        // Factory kept for SqlRepository / startup MigrateAsync.
        void ConfigureSqlite(DbContextOptionsBuilder options) =>
            options.UseSqlite(connectionString);

        services.AddDbContext<SqlContext>(ConfigureSqlite);
        services.AddDbContextFactory<SqlContext>(ConfigureSqlite);

        services.AddScoped(typeof(ISqlRepository<>), typeof(SqlRepository<>));

        // Scoped: one HTTP request → one context, one tool audit, CatalogTools (MediatR).
        services.AddScoped<IToolCallSink, ToolCallSink>();
        services.AddScoped<CatalogTools>();
        services.AddSingleton<IPromptBuilder, PromptBuilder>();
        // Prod history is SQLite. In-memory ChatHistoryStore is for unit tests only.
        services.AddScoped<IChatHistoryStore, EfChatHistoryStore>();
        services.AddScoped<IChatService, ChatService>();

        // UseFunctionInvocation = MEAI runs C# tools when the model requests them.
        services.AddSingleton<IChatClient>(_ =>
        {
            IChatClient ollama = new OllamaChatClient(endpointUri, model);
            return new ChatClientBuilder(ollama)
                .UseFunctionInvocation()
                .Build();
        });

        return services;
    }

    // Creates/opens the DB file and applies migrations (books seed + chat tables). Not ExistsAsync.
    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        var factory = services.GetRequiredService<IDbContextFactory<SqlContext>>();
        await using var db = await factory.CreateDbContextAsync();
        await db.Database.MigrateAsync();
    }
}
