using Common.Domain;
using FluentValidation;
using MediatR;

namespace Common.Application;

/// <summary>
/// Pipeline behavior that validates commands/queries using FluentValidation
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Count != 0)
        {
            var errors = failures
                .Select(f => new Error(f.PropertyName, f.ErrorMessage))
                .ToArray();

            // For Result<T> responses, return a validation failure
            if (typeof(TResponse).IsGenericType &&
                typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var resultType = typeof(TResponse).GetGenericArguments()[0];
                var failureMethod = typeof(Result)
                    .GetMethod(nameof(Result.Failure))!
                    .MakeGenericMethod(resultType);

                var validationError = new Error(
                    "Validation.Error",
                    string.Join("; ", errors.Select(e => e.Message)));

                return (TResponse)failureMethod.Invoke(null, new object[] { validationError })!;
            }

            // For non-Result responses, throw exception
            throw new ValidationException(failures);
        }

        return await next();
    }
}
