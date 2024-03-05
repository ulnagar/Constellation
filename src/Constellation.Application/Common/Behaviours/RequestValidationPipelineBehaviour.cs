namespace Constellation.Application.Common.Behaviours;

using Constellation.Core.Shared;
using FluentValidation;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RequestValidationPipelineBehaviour<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public RequestValidationPipelineBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        if (!_validators.Any())
            return await next();

        List<Error> errors = new();

        foreach (IValidator<TRequest> validator in _validators)
        {
            FluentValidation.Results.ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
            IEnumerable<Error> validationErrors = validationResult.Errors
                .Where(validationFailure => validationFailure is not null)
                .Select(failure => new Error(
                    failure.PropertyName,
                    failure.ErrorMessage))
                .Distinct();

            errors.AddRange(validationErrors);
        }

        if (errors.Any())
        {
            return CreateValidationResult<TResponse>(errors.ToArray());
        }

        return await next();
    }

    private static TResult CreateValidationResult<TResult>(Error[] errors)
    where TResult : Result
    {
        if (typeof(TResult) == typeof(Result))
        {
            return (ValidationResult.WithErrors(errors) as TResult)!;
        }

        object validationResult = typeof(ValidationResult<>)
            .GetGenericTypeDefinition()
            .MakeGenericType(typeof(TResult).GenericTypeArguments[0])
            .GetMethod(nameof(ValidationResult.WithErrors))!
            .Invoke(null, new object?[] { errors })!;

        return (TResult)validationResult;
    }

}