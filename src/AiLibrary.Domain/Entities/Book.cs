namespace AiLibrary.Domain.Entities;

public sealed class Book
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Author { get; set; }
    public required string Genre { get; set; }
    public required string ReadingLevel { get; set; }
    public int PageCount { get; set; }
    public required string Blurb { get; set; }
    public List<string> Tags { get; set; } = [];
}
