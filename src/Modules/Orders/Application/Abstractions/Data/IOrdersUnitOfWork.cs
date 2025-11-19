namespace Modules.Orders.Application.Abstractions.Data;

public interface IOrdersUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
