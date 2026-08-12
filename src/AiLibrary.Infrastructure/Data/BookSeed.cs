using AiLibrary.Domain.Entities;

namespace AiLibrary.Infrastructure.Data;

internal static class BookSeed
{
    // Stable ids so HasData migrations stay deterministic.
    internal static readonly Book[] All =
    [
        new()
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111101"),
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
            Id = Guid.Parse("11111111-1111-1111-1111-111111111102"),
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
            Id = Guid.Parse("11111111-1111-1111-1111-111111111103"),
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
            Id = Guid.Parse("11111111-1111-1111-1111-111111111104"),
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
            Id = Guid.Parse("11111111-1111-1111-1111-111111111105"),
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
            Id = Guid.Parse("11111111-1111-1111-1111-111111111106"),
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
            Id = Guid.Parse("11111111-1111-1111-1111-111111111107"),
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
            Id = Guid.Parse("11111111-1111-1111-1111-111111111108"),
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
            Id = Guid.Parse("11111111-1111-1111-1111-111111111109"),
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
            Id = Guid.Parse("11111111-1111-1111-1111-111111111110"),
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
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
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
            Id = Guid.Parse("11111111-1111-1111-1111-111111111112"),
            Title = "And Then There Were None",
            Author = "Agatha Christie",
            Genre = "Mystery",
            ReadingLevel = "Adult",
            PageCount = 272,
            Blurb = "Ten strangers invited to an island are killed one by one in a masterclass of suspense.",
            Tags = ["thriller", "classic", "isolated"]
        }
    ];
}
