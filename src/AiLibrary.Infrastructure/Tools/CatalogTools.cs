using System.ComponentModel;
using System.Text.Json;
using AiLibrary.Application.Abstractions;
using AiLibrary.Application.Queries.Books;
using MediatR;
using Microsoft.Extensions.AI;

namespace AiLibrary.Infrastructure.Tools;

/// <summary>
/// C# functions the model may call (like Learn's get_current_weather).
/// Business rules stay in MediatR book queries — this only adapts them for MEAI.
/// </summary>
public sealed class CatalogTools
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IMediator _mediator;
    private readonly IToolCallSink _toolCallSink;

    public CatalogTools(IMediator mediator, IToolCallSink toolCallSink)
    {
        _mediator = mediator;
        _toolCallSink = toolCallSink;
    }

    // name/description become the tool schema the model sees.
    public IList<AITool> GetTools() =>
    [
        AIFunctionFactory.Create(
            SearchCatalogAsync,
            name: "search_catalog",
            description: "Search the library catalog by free text and/or genre. Use before recommending in-stock books."),
        AIFunctionFactory.Create(
            GetBookByIdAsync,
            name: "get_book_by_id",
            description: "Get one catalog book by its GUID id.")
    ];

    // [Description] on params → JSON-schema docs for the model.
    [Description("Search the library catalog by free text and/or genre.")]
    public async Task<string> SearchCatalogAsync(
        [Description("Optional text to match title, author, blurb, tags, or genre")] string? query = null,
        [Description("Optional genre filter, e.g. Mystery or Fantasy")] string? genre = null,
        CancellationToken cancellationToken = default)
    {
        // UI chips (not required by MEAI).
        _toolCallSink.Record("search_catalog", FormatDetail(query, genre));

        // Single source of catalog filter logic.
        var books = await _mediator.Send(new GetBooksQuery(query, genre), cancellationToken);
        var payload = books.Take(10).Select(b => new
        {
            b.Id,
            b.Title,
            b.Author,
            b.Genre,
            b.ReadingLevel,
            b.PageCount,
            b.Blurb,
            b.Tags
        });

        // Tool results are text; model reads this JSON on the next round.
        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    [Description("Get one catalog book by its GUID id.")]
    public async Task<string> GetBookByIdAsync(
        [Description("Book id (GUID)")] Guid id,
        CancellationToken cancellationToken = default)
    {
        _toolCallSink.Record("get_book_by_id", id.ToString("D"));

        var book = await _mediator.Send(new GetBookByIdQuery(id), cancellationToken);
        return book is null
            ? """{"error":"Book not found"}"""
            : JsonSerializer.Serialize(book, JsonOptions);
    }

    private static string FormatDetail(string? query, string? genre)
    {
        var bits = new List<string>(2);
        if (!string.IsNullOrWhiteSpace(query))
        {
            bits.Add($"query={query.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(genre))
        {
            bits.Add($"genre={genre.Trim()}");
        }

        return bits.Count == 0 ? "(all)" : string.Join(", ", bits);
    }
}
