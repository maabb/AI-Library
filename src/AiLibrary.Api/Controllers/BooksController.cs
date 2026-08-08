using AiLibrary.Application.Abstractions;
using AiLibrary.Application.Dtos.Books;
using AiLibrary.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AiLibrary.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookCatalog _catalog;

    public BooksController(IBookCatalog catalog)
    {
        _catalog = catalog;
    }

    /// <summary>List catalog books. Optional text and genre filters.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BookDto>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<BookDto>> GetAll(
        [FromQuery] string? q = null,
        [FromQuery] string? genre = null)
    {
        var books = string.IsNullOrWhiteSpace(q) && string.IsNullOrWhiteSpace(genre)
            ? _catalog.GetAll()
            : _catalog.Search(q, genre);

        return Ok(books.Select(Map).ToList());
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<BookDto> GetById(string id)
    {
        var book = _catalog.GetById(id);
        return book is null ? NotFound() : Ok(Map(book));
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
