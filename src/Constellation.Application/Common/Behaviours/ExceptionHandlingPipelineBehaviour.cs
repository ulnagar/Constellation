namespace Constellation.Application.Common.Behaviours;

using MediatR;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ExceptionHandlingPipelineBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
{
    private readonly ILogger _logger;

    public ExceptionHandlingPipelineBehaviour(
        ILogger logger)
    {
        _logger = logger.ForContext<ExceptionHandlingPipelineBehaviour<TRequest, TResponse>>();
    }

    public async Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        try
        {
            return await next();
        }
        catch (Exception e)
        {
            _logger
                .ForContext(nameof(Exception), e, true)
                .Error("Unhandled exception for {request}", typeof(TRequest).Name);

            throw;
        }
    }
}
