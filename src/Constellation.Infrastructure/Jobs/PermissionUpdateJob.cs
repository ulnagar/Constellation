namespace Constellation.Infrastructure.Jobs;

using Application.DTOs;
using Constellation.Application.Features.API.Operations.Commands;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Operations;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class PermissionUpdateJob : IPermissionUpdateJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger _logger;

    public PermissionUpdateJob(
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _logger = logger.ForContext<IPermissionUpdateJob>();
    }

    public async Task StartJob(Guid jobId, CancellationToken token)
    {
        _logger.ForContext("JobId", jobId);

        _logger.Information("Searching for operations...");
        ICollection<CanvasOperation> operations = await _unitOfWork.CanvasOperations.AllToProcess();
        _logger.Information("Found {count} operations to process.", operations.Count);
        
        foreach (CanvasOperation operation in operations)
        {
            if (token.IsCancellationRequested)
                return;

            await ProcessOperation(operation);
        }

        await _unitOfWork.CompleteAsync(token);

        if (token.IsCancellationRequested)
            return;

        _logger.Information("Completed");
    }
    
    private async Task ProcessOperation(CanvasOperation operation)
    {
        ServiceOperationResult<CanvasOperation> result = await _mediator.Send(new ProcessCanvasOperationCommand { Operation = operation });
        foreach (string line in result.Errors)
            _logger.Information("{operation}: {line}", operation, line);
    }
}