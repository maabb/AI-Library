using AiLibrary.Infrastructure.Catalog;

namespace AiLibrary.Tests;

public class BookCatalogTests
{
    private readonly InMemoryBookCatalog _catalog = new();

    [Fact]
    public void GetAll_ReturnsSeededBooks()
    {
        Assert.True(_catalog.GetAll().Count >= 8);
    }

    [Fact]
    public void Search_ByGenre_Filters()
    {
        var mystery = _catalog.Search(null, "Mystery");
        Assert.NotEmpty(mystery);
        Assert.All(mystery, b => Assert.Contains("Mystery", b.Genre, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Search_ByQuery_FindsTitleOrTag()
    {
        var hobbit = _catalog.Search("hobbit");
        Assert.Contains(hobbit, b => b.Title.Contains("Hobbit", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetById_Unknown_ReturnsNull()
    {
        Assert.Null(_catalog.GetById("does-not-exist"));
    }
}
