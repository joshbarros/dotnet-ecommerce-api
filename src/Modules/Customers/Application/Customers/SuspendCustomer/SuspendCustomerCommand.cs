using Common.Application;
using Common.Domain;
using FluentValidation;
using Modules.Customers.Application.Abstractions.Data;
using Modules.Customers.Domain.Customers;

namespace Modules.Customers.Application.Customers.SuspendCustomer;

public sealed record SuspendCustomerCommand(
    Guid CustomerId,
    string Reason) : ICommand;

internal sealed class SuspendCustomerCommandHandler : ICommandHandler<SuspendCustomerCommand>
{
    private readonly ICustomerRepository _repository;
    private readonly ICustomersUnitOfWork _unitOfWork;

    public SuspendCustomerCommandHandler(
        ICustomerRepository repository,
        ICustomersUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(SuspendCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByIdAsync(
            CustomerId.Create(request.CustomerId),
            cancellationToken);

        if (customer is null)
        {
            return Result.Failure(new Error(
                "Customer.NotFound",
                $"Customer with ID '{request.CustomerId}' was not found"));
        }

        var result = customer.Suspend(request.Reason);

        if (result.IsFailure)
        {
            return result;
        }

        _repository.Update(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

internal sealed class SuspendCustomerCommandValidator : AbstractValidator<SuspendCustomerCommand>
{
    public SuspendCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Suspension reason is required")
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters");
    }
}
