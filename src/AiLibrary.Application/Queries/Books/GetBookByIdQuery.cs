using AiLibrary.Application.Abstractions;
using AiLibrary.Application.Dtos.Books;
using AiLibrary.Domain.Entities;
using MediatR;

namespace AiLibrary.Application.Queries.Books;

public record GetBookByIdQuery(Guid Id) : IRequest<BookDto?>;

public sealed class GetBookByIdQueryHandler : IRequestHandler<GetBookByIdQuery, BookDto?>
{
    private readonly ISqlRepository<Book> _books;

    public GetBookByIdQueryHandler(ISqlRepository<Book> books)
    {
        _books = books;
    }

    public async Task<BookDto?> Handle(GetBookByIdQuery request, CancellationToken cancellationToken)
    {
        var book = await _books.GetByIdAsync(request.Id, cancellationToken);
        if (book is null)
        {
            return null;
        }

        return new BookDto
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
}
