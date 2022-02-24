using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;
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

        public async Task StartJob(bool automated)
        {
            if (automated)
            {
                var jobStatus = await _unitOfWork.JobActivations.GetForJob(nameof(IPermissionUpdateJob));
                if (jobStatus == null || !jobStatus.IsActive)
                {
                    _logger.LogWarning("Stopped due to job being set inactive.");
                    return;
                }
            }

            _logger.LogInformation($"Searching for users without Adobe Connect Principal Id information...");
            var updatedUsers = await _adobeConnectService.UpdateUsers();
            _logger.LogInformation($"Found information for {updatedUsers.Count()} users.");

            _logger.LogInformation("");
            _logger.LogInformation($"Searching for operations...");
            var operations = await _unitOfWork.AdobeConnectOperations.AllToProcess();
            var canvasOperations = await _unitOfWork.CanvasOperations.AllToProcess();
            _logger.LogInformation($"Found {operations.Count} operations to process.");

            foreach (var operation in operations)
                await ProcessOperation(operation);

            _logger.LogInformation($"Searching for overdue operations...");
            operations = await _unitOfWork.AdobeConnectOperations.AllOverdue();
            _logger.LogInformation($"Found {operations.Count} overdue operations to process.");

            foreach (var operation in operations)
                await ProcessOperation(operation);

            foreach (var operation in canvasOperations)
                await ProcessOperation(operation);

            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("");
            _logger.LogInformation($"Fixing old and outdated operation entries...");
            await PruneAdobeConnectOperations();
            _logger.LogInformation($"Completed");
        }

        private async Task PruneAdobeConnectOperations()
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

            _logger.LogInformation($" Found {operationCount} perations to update!");

            await _unitOfWork.CompleteAsync();
        }

        private async Task ProcessOperation(AdobeConnectOperation operation)
        {
            var result = await _adobeConnectService.ProcessOperation(operation);
            if (result.Success)
                await _operationsService.MarkAdobeConnectOperationComplete(operation.Id);
            foreach (var line in result.Errors)
                _logger.LogInformation(line);
        }

        private async Task ProcessOperation(CanvasOperation operation)
        {
            var result = await _canvasService.ProcessOperation(operation);
            foreach (var line in result.Errors)
                _logger.LogInformation(line);
        }
    }
}
