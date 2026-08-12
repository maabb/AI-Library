using AiLibrary.Application.Abstractions;
using AiLibrary.Application.Dtos.Books;
using AiLibrary.Domain.Entities;
using MediatR;

namespace AiLibrary.Application.Queries.Books;

public record GetBooksQuery(string? Query, string? Genre) : IRequest<IReadOnlyList<BookDto>>;

public sealed class GetBooksQueryHandler : IRequestHandler<GetBooksQuery, IReadOnlyList<BookDto>>
{
    private readonly ISqlRepository<Book> _books;

    public GetBooksQueryHandler(ISqlRepository<Book> books)
    {
        _books = books;
    }

    public Task<IReadOnlyList<BookDto>> Handle(GetBooksQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Book> query = _books.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Genre))
        {
            var genre = request.Genre.Trim();
            query = query.Where(b => b.Genre.Contains(genre, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var term = request.Query.Trim();
            query = query.Where(b =>
                b.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                b.Author.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                b.Blurb.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                b.Genre.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                b.Tags.Any(t => t.Contains(term, StringComparison.OrdinalIgnoreCase)));
        }

        IReadOnlyList<BookDto> result = query
            .OrderBy(b => b.Title)
            .Select(Map)
            .ToList();

        return Task.FromResult(result);
    }

    private static BookDto Map(Book book) => new()
    {
        Id = book.Id,
        Title = book.Title,
        Author = book.Author,
        Genre = book.Genre,
        ReadingLevel = book.ReadingLevel,
        PageCount = book.PageCount,
        Blurb = book.Blurb,
        Tags = book.Tags
    };
}
