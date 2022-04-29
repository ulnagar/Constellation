using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Jobs
{
    public class PermissionUpdateJob : IPermissionUpdateJob, IScopedService, IHangfireJob
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAdobeConnectService _adobeConnectService;
        private readonly IOperationService _operationsService;
        private readonly ICanvasService _canvasService;
        private readonly ILogger<IPermissionUpdateJob> _logger;

        private Guid JobId { get; set; }

        public PermissionUpdateJob(IUnitOfWork unitOfWork, IAdobeConnectService adobeConnectService,
            IOperationService operationsService, ICanvasService canvasService,
            ILogger<IPermissionUpdateJob> logger)
        {
            _unitOfWork = unitOfWork;
            _adobeConnectService = adobeConnectService;
            _operationsService = operationsService;
            _canvasService = canvasService;
            _logger = logger;
        }

        public async Task StartJob(Guid jobId, CancellationToken token)
        {
            JobId = jobId;

            _logger.LogInformation("{id}: Searching for users without Adobe Connect Principal Id information...", jobId);
            var updatedUsers = await _adobeConnectService.UpdateUsers();
            _logger.LogInformation("{id}: Found Adobe Connect Principal Id information for {count} users.", jobId, updatedUsers.Count());

            _logger.LogInformation("{id}: Searching for operations...", jobId);
            var operations = await _unitOfWork.AdobeConnectOperations.AllToProcess();
            var canvasOperations = await _unitOfWork.CanvasOperations.AllToProcess();
            _logger.LogInformation("{id}: Found {count} operations to process.", jobId, operations.Count);

            foreach (var operation in operations)
            {
                if (token.IsCancellationRequested)
                    return;

                await ProcessOperation(operation);
            }

            _logger.LogInformation("{id}: Searching for overdue operations...", jobId);
            operations = await _unitOfWork.AdobeConnectOperations.AllOverdue();
            _logger.LogInformation("{id}: Found {count} overdue operations to process.", jobId, operations.Count);

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

            _logger.LogInformation("{id}: Fixing old and outdated operation entries...", jobId);
            await PruneAdobeConnectOperations(token);
            _logger.LogInformation("{id}: Completed", jobId);
        }

        private async Task PruneAdobeConnectOperations(CancellationToken token)
        {
            var covers = await _unitOfWork.Covers.ForOperationCancellation();
            var operationCount = 0;

            foreach (var cover in covers)
            {
                var operations = cover.AdobeConnectOperations.Where(op => !op.IsCompleted && !op.IsDeleted);

                foreach (var operation in operations)
                {
                    operationCount++;
                    await _operationsService.CancelAdobeConnectOperation(operation.Id);
                }
            }

            _logger.LogInformation("{id}: Found {operationCount} operations to cancel!", JobId, operationCount);

            await _unitOfWork.CompleteAsync(token);
        }

        private async Task ProcessOperation(AdobeConnectOperation operation)
        {
            var result = await _adobeConnectService.ProcessOperation(operation);
            if (result.Success)
                await _operationsService.MarkAdobeConnectOperationComplete(operation.Id);
            foreach (var line in result.Errors)
                _logger.LogInformation("{id}: {operation}: {line}", JobId, operation, line);
        }

        private async Task ProcessOperation(CanvasOperation operation)
        {
            var result = await _canvasService.ProcessOperation(operation);
            foreach (var line in result.Errors)
                _logger.LogInformation("{id}: {operation}: {line}", JobId, operation, line);
        }
    }
}
