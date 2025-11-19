using Common.Application;
using Common.Domain;
using FluentValidation;
using Modules.Customers.Application.Abstractions.Data;
using Modules.Customers.Domain.Common;
using Modules.Customers.Domain.Customers;

namespace Modules.Customers.Application.Customers.UpdateCustomer;

public sealed record UpdateCustomerCommand(
    Guid CustomerId,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber) : ICommand;

internal sealed class UpdateCustomerCommandHandler : ICommandHandler<UpdateCustomerCommand>
{
    private readonly ICustomerRepository _repository;
    private readonly ICustomersUnitOfWork _unitOfWork;

    public UpdateCustomerCommandHandler(
        ICustomerRepository repository,
        ICustomersUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
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

        var email = Email.Create(request.Email);

        // Check if email is being changed and if it's already taken
        if (customer.Email.Value != email.Value)
        {
            var existingCustomer = await _repository.GetByEmailAsync(email, cancellationToken);
            if (existingCustomer is not null)
            {
                return Result.Failure(new Error(
                    "Customer.EmailAlreadyExists",
                    $"Customer with email '{request.Email}' already exists"));
            }
        }

        var name = CustomerName.Create(request.FirstName, request.LastName);
        var phoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber)
            ? null
            : PhoneNumber.Create(request.PhoneNumber);

        var result = customer.UpdateContactInfo(name, email, phoneNumber);

        if (result.IsFailure)
        {
            return result;
        }

        _repository.Update(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

internal sealed class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MinimumLength(2).WithMessage("First name must be at least 2 characters")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MinimumLength(2).WithMessage("Last name must be at least 2 characters")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email is not in a valid format")
            .MaximumLength(254).WithMessage("Email must not exceed 254 characters");
    }
}
