using AiLibrary.Application.Abstractions;
using AiLibrary.Infrastructure.Data;
using AiLibrary.Infrastructure.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AiLibrary.Tests.Support;

/// <summary>Shared in-memory SQLite + generic <see cref="ISqlRepository{TEntity}"/> for unit tests.</summary>
public sealed class TestSqlFixture : IDisposable
{
    private readonly SqliteConnection _connection;

    public ServiceProvider Services { get; }

    public TestSqlFixture()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        services.AddDbContextFactory<SqlContext>(options => options.UseSqlite(_connection));
        services.AddScoped(typeof(ISqlRepository<>), typeof(SqlRepository<>));

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
