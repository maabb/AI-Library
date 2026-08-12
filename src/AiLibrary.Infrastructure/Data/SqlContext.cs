using AiLibrary.Domain.Entities;
using AiLibrary.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AiLibrary.Infrastructure.Data;

// Single SQLite database: books catalog + durable chat history.
public sealed class SqlContext : DbContext
{
    public SqlContext(DbContextOptions<SqlContext> options) : base(options)
    {
    }

    // Prefer these over _db.Set<T>() — clearer and typed.
    public DbSet<Book> Books => Set<Book>();
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<ChatMessageRow> ChatMessages => Set<ChatMessageRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureBooks(modelBuilder);
        ConfigureChat(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    private static void ConfigureBooks(ModelBuilder modelBuilder)
    {
        var book = modelBuilder.Entity<Book>();

        book.ToTable("Books");
        book.HasKey(b => b.Id);
        book.Property(b => b.Title).HasMaxLength(200).IsRequired();
        book.Property(b => b.Author).HasMaxLength(200).IsRequired();
        book.Property(b => b.Genre).HasMaxLength(100).IsRequired();
        book.Property(b => b.ReadingLevel).HasMaxLength(100).IsRequired();
        book.Property(b => b.Blurb).HasMaxLength(2000).IsRequired();
        // Tags stored as one TEXT column (unit-separator joined).
        book.Property(b => b.Tags)
            .HasConversion(
                tags => string.Join('\u001f', tags),
                value => string.IsNullOrEmpty(value)
                    ? new List<string>()
                    : value.Split('\u001f', StringSplitOptions.RemoveEmptyEntries).ToList(),
                new ValueComparer<List<string>>(
                    (a, b) => a!.SequenceEqual(b!),
                    v => v.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode(StringComparison.Ordinal))),
                    v => v.ToList()));

        // Seed catalog on migrate / EnsureCreated.
        book.HasData(BookSeed.All);
    }

    private static void ConfigureChat(ModelBuilder modelBuilder)
    {
        var session = modelBuilder.Entity<ChatSession>();
        session.ToTable("ChatSessions");
        session.HasKey(s => s.Id);
        session.Property(s => s.Id).HasMaxLength(64);
        session.HasIndex(s => s.UpdatedAt);

        var message = modelBuilder.Entity<ChatMessageRow>();
        message.ToTable("ChatMessages");
        message.HasKey(m => m.Id);
        message.Property(m => m.SessionId).HasMaxLength(64).IsRequired();
        message.Property(m => m.Role).HasMaxLength(32).IsRequired();
        message.Property(m => m.Content).IsRequired();
        // One sequence number per message within a session.
        message.HasIndex(m => new { m.SessionId, m.Sequence }).IsUnique();

        // Deleting a session removes its messages.
        message.HasOne(m => m.Session)
            .WithMany(s => s.Messages)
            .HasForeignKey(m => m.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
