namespace Constellation.Application.Common.Behaviours;

using Core.Shared;
using MediatR;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RequestLoggingPipelineBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
    where TResponse : Result
{
    private readonly ILogger _logger;

    public RequestLoggingPipelineBehaviour(
        ILogger logger)
    {
        _logger = logger.ForContext<RequestLoggingPipelineBehaviour<TRequest, TResponse>>();
    }

    public async Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        string requestName = typeof(TRequest).Name;

        _logger
            .ForContext(requestName, request, true)
            .Information("Processing Request {request}", requestName);

        TResponse result = await next();

        if (result.IsSuccess)
            _logger
                .ForContext(requestName, request, true)
                .ForContext(typeof(TResponse).Name, result, true)
                .Information("Completed request {request}", requestName);
        else
            _logger
                .ForContext(requestName, request, true)
                .ForContext(typeof(TResponse).Name, result, true)
                .ForContext(nameof(Error), result.Error, true)
                .Information("Failed to complete request {request}", requestName);

        return result;
    }
}
