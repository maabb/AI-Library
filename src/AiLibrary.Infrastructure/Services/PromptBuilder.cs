using System.Text;
using AiLibrary.Application.Abstractions;
using Microsoft.Extensions.AI;

namespace AiLibrary.Infrastructure.Services;

public class PromptBuilder : IPromptBuilder
{
    private readonly IBookCatalog _catalog;

    public PromptBuilder(IBookCatalog catalog)
    {
        _catalog = catalog;
    }

    public ChatMessage GetSystemMessage()
    {
        var catalogBlock = BuildCatalogBlock();

        return new ChatMessage(ChatRole.System, $"""
            You are Ava, a friendly and knowledgeable librarian assistant for an AI-powered library.

            Your goals:
            - Help readers discover books they will enjoy.
            - Prefer titles from the LIBRARY CATALOG below when recommending.
            - You may mention well-known books outside the catalog, but clearly say they are not in stock here.
            - Give clear, concise recommendations and short spoiler-free summaries when asked.
            - Remember details the reader shares earlier in the conversation and use them.

            When someone first greets you, introduce yourself briefly as the library assistant.

            When recommending books, try to learn (if not already known):
            1. Topics or genres they like (or dislike)
            2. Age group or reading level
            3. Mood, length, or time they have to read

            Recommendation style:
            - Suggest up to three titles unless they ask for more.
            - For each title: name, author, and one sentence on why it fits.
            - Prefer catalog matches; cite genre/reading level from the catalog when useful.
            - Offer a short spoiler-free teaser, not a full plot dump, unless they ask for a summary.
            - If you are unsure about a factual detail, say so honestly rather than inventing.

            Conversation style:
            - Warm, clear, and practical — like a great librarian at the desk.
            - Ask at most one or two clarifying questions at a time.
            - At the end of helpful replies, invite a follow-up (similar books, summary, difficulty, etc.).

            LIBRARY CATALOG (authoritative inventory for this app):
            {catalogBlock}
            """);
    }

    private string BuildCatalogBlock()
    {
        var sb = new StringBuilder();
        foreach (var book in _catalog.GetAll())
        {
            sb.Append("- ")
                .Append(book.Title)
                .Append(" by ")
                .Append(book.Author)
                .Append(" | Genre: ")
                .Append(book.Genre)
                .Append(" | Level: ")
                .Append(book.ReadingLevel)
                .Append(" | Pages: ")
                .Append(book.PageCount)
                .Append(" | ")
                .Append(book.Blurb);

            if (book.Tags.Count > 0)
            {
                sb.Append(" | Tags: ").Append(string.Join(", ", book.Tags));
            }

            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }
}
