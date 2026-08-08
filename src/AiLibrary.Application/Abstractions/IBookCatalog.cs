using AiLibrary.Domain.Entities;

namespace AiLibrary.Application.Abstractions;

public interface IBookCatalog
{
    IReadOnlyList<Book> GetAll();
    Book? GetById(string id);
    IReadOnlyList<Book> Search(string? query, string? genre = null);
}
