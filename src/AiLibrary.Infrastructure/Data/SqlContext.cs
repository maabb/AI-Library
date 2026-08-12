using AiLibrary.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AiLibrary.Infrastructure.Data;

public sealed class SqlContext : DbContext
{
    public SqlContext(DbContextOptions<SqlContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var book = modelBuilder.Entity<Book>();

        book.ToTable("Books");
        book.HasKey(b => b.Id);
        book.Property(b => b.Title).HasMaxLength(200).IsRequired();
        book.Property(b => b.Author).HasMaxLength(200).IsRequired();
        book.Property(b => b.Genre).HasMaxLength(100).IsRequired();
        book.Property(b => b.ReadingLevel).HasMaxLength(100).IsRequired();
        book.Property(b => b.Blurb).HasMaxLength(2000).IsRequired();
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

        book.HasData(BookSeed.All);

        base.OnModelCreating(modelBuilder);
    }
}
