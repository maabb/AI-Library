using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AiLibrary.Infrastructure.Data;

public sealed class SqlContextFactory : IDesignTimeDbContextFactory<SqlContext>
{
    public SqlContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SqlContext>()
            .UseSqlite("Data Source=ailibrary")
            .Options;

        return new SqlContext(options);
    }
}
