namespace AiLibrary.Application.Dtos.Books;

public sealed class BookDto
{
    public Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Author { get; init; }
    public required string Genre { get; init; }
    public required string ReadingLevel { get; init; }
    public int PageCount { get; init; }
    public required string Blurb { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = [];
}
