using Constellation.Application.Common.ValidationRules;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Common.Behaviours
{
    public class BusinessValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TResponse : class
        where TRequest : IRequest<TResponse>, IValidatable
    {
        private readonly IValidator<TRequest> _compositeValidator;
        private readonly ILogger<TRequest> _logger;

        public BusinessValidationBehaviour(IValidator<TRequest> compositeValidator, ILogger<TRequest> logger)
        {
            _compositeValidator = compositeValidator;
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var result = await _compositeValidator.ValidateAsync(request, cancellationToken);

            if (!result.IsValid)
            {
                _logger.LogError("Validation Error!", result.Errors.Select(s => s.ErrorMessage).Aggregate((acc, curr) => acc += string.Concat("_|_", curr)));

                var responseType = typeof(TResponse);

                if (responseType.IsGenericType)
                {
                    var resultType = responseType.GetGenericArguments()[0];
                    var invalidResponseType = typeof(ValidateableResponse<>).MakeGenericType(resultType);

                    var invalidResponse =
                        Activator.CreateInstance(invalidResponseType, null, result.Errors.Select(s => s.ErrorMessage).ToList()) as TResponse;

                    return invalidResponse;
                } else
                {
                    var invalidResponse = new ValidateableResponse(result.Errors.Select(s => s.ErrorMessage).ToList()) as TResponse;

                    return invalidResponse;
                }
            }

            var response = await next();

            return response;
        }
    }
}


