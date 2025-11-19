using Common.Application;
using Common.Domain;
using FluentValidation;
using Modules.Customers.Application.Abstractions.Data;
using Modules.Customers.Domain.Common;
using Modules.Customers.Domain.Customers;

namespace Modules.Customers.Application.Customers.CreateCustomer;

public sealed record CreateCustomerCommand(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber) : ICommand<Guid>;

internal sealed class CreateCustomerCommandHandler : ICommandHandler<CreateCustomerCommand, Guid>
{
    private readonly ICustomerRepository _repository;
    private readonly ICustomersUnitOfWork _unitOfWork;

    public CreateCustomerCommandHandler(
        ICustomerRepository repository,
        ICustomersUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);

        // Check if customer with email already exists
        var existingCustomer = await _repository.GetByEmailAsync(email, cancellationToken);
        if (existingCustomer is not null)
        {
            return Result.Failure<Guid>(new Error(
                "Customer.EmailAlreadyExists",
                $"Customer with email '{request.Email}' already exists"));
        }

        var name = CustomerName.Create(request.FirstName, request.LastName);
        var phoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber)
            ? null
            : PhoneNumber.Create(request.PhoneNumber);

        var customer = Customer.Create(name, email, phoneNumber);

        await _repository.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success((Guid)customer.Id);
    }
}

internal sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
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

        RuleFor(x => x.PhoneNumber)
            .Must(phone => string.IsNullOrWhiteSpace(phone) || phone.Length >= 10)
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("Phone number must be at least 10 characters");
    }
}
