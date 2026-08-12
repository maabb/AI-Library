using AiLibrary.Application.Abstractions;
using AiLibrary.Infrastructure.Tools;
using AiLibrary.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace AiLibrary.Tests;

public class BookCatalogToolsTests : IDisposable
{
    private readonly TestSqlFixture _fx = new();

    [Fact]
    public async Task SearchCatalog_ReturnsSeededMatches_AndRecordsTool()
    {
        using var scope = _fx.CreateScope();
        var tools = scope.ServiceProvider.GetRequiredService<CatalogTools>();
        var sink = scope.ServiceProvider.GetRequiredService<IToolCallSink>();

        var json = await tools.SearchCatalogAsync("hobbit", null, CancellationToken.None);

        Assert.Contains("Hobbit", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(sink.Snapshot(), t => t.Name == "search_catalog");
    }

    [Fact]
    public async Task GetBookById_Unknown_ReturnsErrorJson()
    {
        using var scope = _fx.CreateScope();
        var tools = scope.ServiceProvider.GetRequiredService<CatalogTools>();
        var sink = scope.ServiceProvider.GetRequiredService<IToolCallSink>();

        var json = await tools.GetBookByIdAsync(
            Guid.Parse("99999999-9999-9999-9999-999999999999"),
            CancellationToken.None);

        Assert.Contains("not found", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(sink.Snapshot(), t => t.Name == "get_book_by_id");
    }

    [Fact]
    public void GetTools_ExposesTwoMeaiFunctions()
    {
        using var scope = _fx.CreateScope();
        var tools = scope.ServiceProvider.GetRequiredService<CatalogTools>();

        Assert.Equal(2, tools.GetTools().Count);
    }

    public void Dispose() => _fx.Dispose();
}
