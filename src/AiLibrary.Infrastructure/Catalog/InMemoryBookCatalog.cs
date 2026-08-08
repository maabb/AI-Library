using AiLibrary.Application.Abstractions;
using AiLibrary.Domain.Entities;

namespace AiLibrary.Infrastructure.Catalog;

/// <summary>
/// Seeded mini-catalog so the librarian can ground recommendations in real inventory.
/// Replace with EF Core / SQLite when you move past the learning stage.
/// </summary>
public sealed class InMemoryBookCatalog : IBookCatalog
{
    private readonly IReadOnlyList<Book> _books =
    [
        new()
        {
            Id = "hobbit",
            Title = "The Hobbit",
            Author = "J.R.R. Tolkien",
            Genre = "Fantasy",
            ReadingLevel = "Middle grade / YA+",
            PageCount = 310,
            Blurb = "A reluctant hobbit joins dwarves on a treasure quest filled with wit and wonder.",
            Tags = ["adventure", "classic", "quest", "dragons"]
        },
        new()
        {
            Id = "earthsea",
            Title = "A Wizard of Earthsea",
            Author = "Ursula K. Le Guin",
            Genre = "Fantasy",
            ReadingLevel = "YA / Adult",
            PageCount = 183,
            Blurb = "A gifted young mage must face the shadow unleashed by his own pride.",
            Tags = ["magic", "coming-of-age", "classic"]
        },
        new()
        {
            Id = "pride",
            Title = "Pride and Prejudice",
            Author = "Jane Austen",
            Genre = "Romance / Classic",
            ReadingLevel = "Adult",
            PageCount = 432,
            Blurb = "Sharp social comedy about first impressions, pride, and unlikely affection.",
            Tags = ["romance", "classic", "wit"]
        },
        new()
        {
            Id = "orient",
            Title = "Murder on the Orient Express",
            Author = "Agatha Christie",
            Genre = "Mystery",
            ReadingLevel = "Adult",
            PageCount = 256,
            Blurb = "Hercule Poirot investigates a locked-train murder with too many suspects.",
            Tags = ["detective", "classic", "puzzle"]
        },
        new()
        {
            Id = "dune",
            Title = "Dune",
            Author = "Frank Herbert",
            Genre = "Science Fiction",
            ReadingLevel = "Adult",
            PageCount = 688,
            Blurb = "Political intrigue and destiny collide on a desert planet that holds the universe's most valuable resource.",
            Tags = ["epic", "politics", "worldbuilding"]
        },
        new()
        {
            Id = "gatsby",
            Title = "The Great Gatsby",
            Author = "F. Scott Fitzgerald",
            Genre = "Literary Fiction",
            ReadingLevel = "Adult / YA+",
            PageCount = 180,
            Blurb = "Jazz Age tragedy of longing, wealth, and the American dream.",
            Tags = ["classic", "short", "tragedy"]
        },
        new()
        {
            Id = "kindred",
            Title = "Kindred",
            Author = "Octavia E. Butler",
            Genre = "Science Fiction / Historical",
            ReadingLevel = "Adult",
            PageCount = 287,
            Blurb = "A modern woman is pulled through time into the brutal realities of antebellum slavery.",
            Tags = ["time-travel", "powerful", "thoughtful"]
        },
        new()
        {
            Id = "circe",
            Title = "Circe",
            Author = "Madeline Miller",
            Genre = "Mythic Fiction",
            ReadingLevel = "Adult",
            PageCount = 393,
            Blurb = "A vivid reimagining of the witch of Aiaia finding power and voice among gods and mortals.",
            Tags = ["mythology", "feminist", "lush-prose"]
        },
        new()
        {
            Id = "project-hail-mary",
            Title = "Project Hail Mary",
            Author = "Andy Weir",
            Genre = "Science Fiction",
            ReadingLevel = "Adult",
            PageCount = 476,
            Blurb = "A lone astronaut wakes on a mission to save Earth — with science, humor, and unlikely friendship.",
            Tags = ["space", "problem-solving", "fun"]
        },
        new()
        {
            Id = "anne",
            Title = "Anne of Green Gables",
            Author = "L.M. Montgomery",
            Genre = "Classic / Coming of Age",
            ReadingLevel = "Middle grade / YA",
            PageCount = 320,
            Blurb = "Imaginative orphan Anne Shirley transforms a quiet farm community with spirit and heart.",
            Tags = ["cozy", "classic", "friendship"]
        },
        new()
        {
            Id = "night-circus",
            Title = "The Night Circus",
            Author = "Erin Morgenstern",
            Genre = "Fantasy",
            ReadingLevel = "Adult / YA+",
            PageCount = 387,
            Blurb = "A mysterious circus becomes the stage for a magical competition and a forbidden romance.",
            Tags = ["atmospheric", "romance", "magic"]
        },
        new()
        {
            Id = "and-then-there-were-none",
            Title = "And Then There Were None",
            Author = "Agatha Christie",
            Genre = "Mystery",
            ReadingLevel = "Adult",
            PageCount = 272,
            Blurb = "Ten strangers invited to an island are killed one by one in a masterclass of suspense.",
            Tags = ["thriller", "classic", "isolated"]
        }
    ];

    public IReadOnlyList<Book> GetAll() => _books;

    public Book? GetById(string id) =>
        _books.FirstOrDefault(b => string.Equals(b.Id, id, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<Book> Search(string? query, string? genre = null)
    {
        IEnumerable<Book> results = _books;

        if (!string.IsNullOrWhiteSpace(genre))
        {
            results = results.Where(b =>
                b.Genre.Contains(genre, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            results = results.Where(b =>
                b.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                b.Author.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                b.Blurb.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                b.Tags.Any(t => t.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                b.Genre.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        return results.ToList();
    }
}
