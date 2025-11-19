namespace Modules.Customers.Application.Abstractions.Data;

public interface ICustomersUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
