using AiLibrary.Application.Abstractions;
using AiLibrary.Application.Queries.Books;
using AiLibrary.Domain.Entities;
using AiLibrary.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace AiLibrary.Tests;

public class BookCatalogTests : IDisposable
{
    private readonly TestSqlFixture _fx = new();
    private readonly IServiceScope _scope;
    private readonly ISqlRepository<Book> _books;

    public BookCatalogTests()
    {
        _scope = _fx.CreateScope();
        _books = _scope.ServiceProvider.GetRequiredService<ISqlRepository<Book>>();
    }

    [Fact]
    public async Task GetBooksQuery_ReturnsSeededBooks()
    {
        var handler = new GetBooksQueryHandler(_books);
        var result = await handler.Handle(new GetBooksQuery(null, null), CancellationToken.None);
        Assert.True(result.Count >= 8);
    }

    [Fact]
    public async Task GetBooksQuery_ByGenre_Filters()
    {
        var handler = new GetBooksQueryHandler(_books);
        var mystery = await handler.Handle(new GetBooksQuery(null, "Mystery"), CancellationToken.None);

        Assert.NotEmpty(mystery);
        Assert.All(mystery, b => Assert.Contains("Mystery", b.Genre, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetBooksQuery_ByQuery_FindsTitle()
    {
        var handler = new GetBooksQueryHandler(_books);
        var hobbit = await handler.Handle(new GetBooksQuery("hobbit", null), CancellationToken.None);

        Assert.Contains(hobbit, b => b.Title.Contains("Hobbit", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetBookByIdQuery_Unknown_ReturnsNull()
    {
        var handler = new GetBookByIdQueryHandler(_books);
        var book = await handler.Handle(
            new GetBookByIdQuery(Guid.Parse("99999999-9999-9999-9999-999999999999")),
            CancellationToken.None);

        Assert.Null(book);
    }

    public void Dispose()
    {
        _scope.Dispose();
        _fx.Dispose();
    }
}
