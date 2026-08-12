using AiLibrary.Application.Abstractions;
using AiLibrary.Application.Queries.Books;
using AiLibrary.Infrastructure.Data;
using AiLibrary.Infrastructure.Data.Repositories;
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
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContextFactory<SqlContext>(options => options.UseSqlite(_connection));
        services.AddScoped(typeof(ISqlRepository<>), typeof(SqlRepository<>));
        services.AddScoped<IToolCallSink, ToolCallSink>();
        services.AddScoped<CatalogTools>();
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
