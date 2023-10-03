namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.Features.API.Operations.Commands;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class PermissionUpdateJob : IPermissionUpdateJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAdobeConnectService _adobeConnectService;
    private readonly IOperationService _operationsService;
    private readonly IMediator _mediator;
    private readonly ILogger _logger;

    private Guid JobId { get; set; }

    public PermissionUpdateJob(
        IUnitOfWork unitOfWork, 
        IAdobeConnectService adobeConnectService,
        IOperationService operationsService, 
        IMediator mediator,
        ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _adobeConnectService = adobeConnectService;
        _operationsService = operationsService;
        _mediator = mediator;
        _logger = logger.ForContext<IPermissionUpdateJob>();
    }

    public async Task StartJob(Guid jobId, CancellationToken token)
    {
        JobId = jobId;

        _logger.Information("{id}: Searching for users without Adobe Connect Principal Id information...", jobId);
        var updatedUsers = await _adobeConnectService.UpdateUsers();
        _logger.Information("{id}: Found Adobe Connect Principal Id information for {count} users.", jobId, updatedUsers.Count());

        _logger.Information("{id}: Searching for operations...", jobId);
        var operations = await _unitOfWork.AdobeConnectOperations.AllToProcess();
        var canvasOperations = await _unitOfWork.CanvasOperations.AllToProcess();
        _logger.Information("{id}: Found {count} operations to process.", jobId, operations.Count);

        foreach (var operation in operations)
        {
            if (token.IsCancellationRequested)
                return;

            await ProcessOperation(operation);
        }

        _logger.Information("{id}: Searching for overdue operations...", jobId);
        operations = await _unitOfWork.AdobeConnectOperations.AllOverdue();
        _logger.Information("{id}: Found {count} overdue operations to process.", jobId, operations.Count);

        foreach (var operation in operations)
        {
            if (token.IsCancellationRequested)
                return;

            await ProcessOperation(operation);
        }

        foreach (var operation in canvasOperations)
        {
            if (token.IsCancellationRequested)
                return;

            await ProcessOperation(operation);
        }

        await _unitOfWork.CompleteAsync(token);

        if (token.IsCancellationRequested)
            return;

        _logger.Information("{id}: Completed", jobId);
    }


    private async Task ProcessOperation(AdobeConnectOperation operation)
    {
        var result = await _adobeConnectService.ProcessOperation(operation);
        if (result.Success)
            await _operationsService.MarkAdobeConnectOperationComplete(operation.Id);
        foreach (var line in result.Errors)
            _logger.Information("{id}: {operation}: {line}", JobId, operation, line);
    }

    private async Task ProcessOperation(CanvasOperation operation)
    {
        var result = await _mediator.Send(new ProcessCanvasOperationCommand { Operation = operation });
        foreach (var line in result.Errors)
            _logger.Information("{id}: {operation}: {line}", JobId, operation, line);
    }
}