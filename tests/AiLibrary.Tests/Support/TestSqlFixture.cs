using AiLibrary.Application.Abstractions;
using AiLibrary.Application.Queries.Books;
using AiLibrary.Infrastructure.Data;
using AiLibrary.Infrastructure.Data.Repositories;
using AiLibrary.Infrastructure.Services;
using AiLibrary.Infrastructure.Tools;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AiLibrary.Tests.Support;

public sealed class TestSqlFixture : IDisposable
{
    private readonly SqliteConnection _connection;

    public ServiceProvider Services { get; }

    public TestSqlFixture()
    {
        // Keep connection open so all scopes share the same in-memory database.
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();

        void ConfigureSqlite(DbContextOptionsBuilder options) =>
            options.UseSqlite(_connection);

        services.AddDbContext<SqlContext>(ConfigureSqlite);
        services.AddDbContextFactory<SqlContext>(ConfigureSqlite);
        services.AddScoped(typeof(ISqlRepository<>), typeof(SqlRepository<>));
        services.AddScoped<IToolCallSink, ToolCallSink>();
        services.AddScoped<CatalogTools>();
        services.AddSingleton<IPromptBuilder, PromptBuilder>();
        services.AddScoped<IChatHistoryStore, EfChatHistoryStore>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetBooksQuery).Assembly));

        Services = services.BuildServiceProvider();

        using var db = Services.GetRequiredService<IDbContextFactory<SqlContext>>().CreateDbContext();
        db.Database.EnsureCreated();
    }

    public IServiceScope CreateScope() => Services.CreateScope();

    public void Dispose()
    {
        Services.Dispose();
        _connection.Dispose();
    }
}
