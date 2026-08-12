using System.Collections;
using System.Linq.Expressions;
using AiLibrary.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AiLibrary.Infrastructure.Data.Repositories;

public sealed class SqlRepository<TEntity> : ISqlRepository<TEntity>, IAsyncDisposable, IDisposable
    where TEntity : class
{
    private readonly SqlContext _context;
    private readonly DbSet<TEntity> _dbSet;

    public SqlRepository(IDbContextFactory<SqlContext> dbFactory)
    {
        _context = dbFactory.CreateDbContext();
        _dbSet = _context.Set<TEntity>();
    }

    public Type ElementType => ((IQueryable<TEntity>)_dbSet).ElementType;
    public Expression Expression => ((IQueryable<TEntity>)_dbSet).Expression;
    public IQueryProvider Provider => ((IQueryable<TEntity>)_dbSet).Provider;

    public IEnumerator<TEntity> GetEnumerator() => ((IEnumerable<TEntity>)_dbSet).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public async Task<TEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken,
        bool withTracking = false)
    {
        if (withTracking)
        {
            return await _dbSet.FindAsync([id], cancellationToken);
        }

        return await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id, cancellationToken);
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    public void Update(TEntity entity) => _dbSet.Update(entity);

    public void Delete(TEntity entity) => _dbSet.Remove(entity);

    public IQueryable<TEntity> AsQueryable() => _dbSet.AsNoTracking();

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);

    public void Dispose() => _context.Dispose();

    public ValueTask DisposeAsync() => _context.DisposeAsync();
}
