using Common.Application;
using Common.Domain;
using FluentValidation;
using Modules.Customers.Application.Abstractions.Data;
using Modules.Customers.Domain.Common;
using Modules.Customers.Domain.Customers;

namespace Modules.Customers.Application.Customers.AddAddress;

public sealed record AddAddressCommand(
    Guid CustomerId,
    string Street,
    string City,
    string State,
    string Country,
    string PostalCode) : ICommand;

internal sealed class AddAddressCommandHandler : ICommandHandler<AddAddressCommand>
{
    private readonly ICustomerRepository _repository;
    private readonly ICustomersUnitOfWork _unitOfWork;

    public AddAddressCommandHandler(
        ICustomerRepository repository,
        ICustomersUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddAddressCommand request, CancellationToken cancellationToken)
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

        var address = Address.Create(
            request.Street,
            request.City,
            request.State,
            request.Country,
            request.PostalCode);

        var result = customer.AddAddress(address);

        if (result.IsFailure)
        {
            return result;
        }

        _repository.Update(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

internal sealed class AddAddressCommandValidator : AbstractValidator<AddAddressCommand>
{
    public AddAddressCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Street is required")
            .MaximumLength(200).WithMessage("Street must not exceed 200 characters");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(100).WithMessage("City must not exceed 100 characters");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("State is required")
            .MaximumLength(100).WithMessage("State must not exceed 100 characters");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required")
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters");

        RuleFor(x => x.PostalCode)
            .NotEmpty().WithMessage("Postal code is required")
            .MaximumLength(20).WithMessage("Postal code must not exceed 20 characters");
    }
}
