namespace AiLibrary.Application.Abstractions;

public interface ISqlRepository<TEntity> : IQueryable<TEntity>
    where TEntity : class
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken, bool withTracking = false);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken);
    void Update(TEntity entity);
    void Delete(TEntity entity);
    IQueryable<TEntity> AsQueryable();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
