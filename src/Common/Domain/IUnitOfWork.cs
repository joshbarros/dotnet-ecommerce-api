namespace Common.Domain;

/// <summary>
/// Unit of Work pattern for coordinating writes across aggregates
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
