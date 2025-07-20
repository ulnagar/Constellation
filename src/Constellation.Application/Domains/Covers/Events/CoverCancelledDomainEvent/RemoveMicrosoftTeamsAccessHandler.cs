namespace Constellation.Application.Domains.Covers.Events.CoverCancelledDomainEvent;

using Abstractions.Messaging;
using Constellation.Core.Models.Covers.Events;
using Constellation.Core.Models.Covers.Repositories;
using Core.Enums;
using Core.Models;
using Core.Models.Covers;
using Core.Models.Covers.Enums;
using Core.Models.StaffMembers.Identifiers;
using Interfaces.Repositories;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveMicrosoftTeamsAccessHandler
    : IDomainEventHandler<CoverCancelledDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMSTeamOperationsRepository _operationsRepository;
    private readonly ICoverRepository _coverRepository;
    private readonly ILogger _logger;

    public RemoveMicrosoftTeamsAccessHandler(
        IUnitOfWork unitOfWork,
        IMSTeamOperationsRepository operationsRepository,
        ICoverRepository coverRepository,
        ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _operationsRepository = operationsRepository;
        _coverRepository = coverRepository;
        _logger = logger.ForContext<CoverCancelledDomainEvent>();
    }

    public async Task Handle(CoverCancelledDomainEvent notification, CancellationToken cancellationToken)
    {
        Cover cover = await _coverRepository.GetById(notification.CoverId, cancellationToken);

        if (cover is null)
        {
            _logger.Warning("{action}: Could not find cover with Id {id} in database", nameof(RemoveMicrosoftTeamsAccessHandler), notification.CoverId);

            return;
        }

        List<MSTeamOperation> existingRequests = await _operationsRepository
            .GetByCoverId(notification.CoverId, cancellationToken);

        List<MSTeamOperation> addRequests = existingRequests
            .Where(operation =>
                operation.Action == MSTeamOperationAction.Add)
            .ToList();

        List<MSTeamOperation> removeRequests = existingRequests
            .Where(operation =>
                operation.Action == MSTeamOperationAction.Remove)
            .ToList();

        // In the set of operations for this cover, has access been granted and not yet revoked?
        bool accessGranted = addRequests
                .Any(operation => operation.IsCompleted);

        bool accessRevoked = removeRequests
                .Any(operation => operation.IsCompleted);

        if (accessGranted && !accessRevoked)
        {
            // Set the first un-actioned remove request to process asap
            MSTeamOperation removeOperation = removeRequests.FirstOrDefault(operation => !operation.IsCompleted && !operation.IsDeleted);

            if (removeOperation is null)
            {
                // Create new operation to remove access
                if (cover.TeacherType == CoverTeacherType.Casual)
                {
                    removeOperation = new CasualMSTeamOperation
                    {
                        OfferingId = cover.OfferingId,
                        CasualId = Guid.Parse(cover.TeacherId),
                        CoverId = cover.Id.Value,
                        Action = MSTeamOperationAction.Remove,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = DateTime.Now
                    };

                    _operationsRepository.Insert(removeOperation);
                }
                else
                {
                    bool success = Guid.TryParse(cover.TeacherId, out Guid staffIdGuid);
                    StaffId staffId = success
                        ? StaffId.FromValue(staffIdGuid)
                        : StaffId.Empty;

                    removeOperation = new TeacherMSTeamOperation
                    {
                        OfferingId = cover.OfferingId,
                        StaffId = staffId,
                        CoverId = cover.Id.Value,
                        Action = MSTeamOperationAction.Remove,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = DateTime.Now
                    };

                    _operationsRepository.Insert(removeOperation);
                }
            }
            else
            {
                removeOperation.DateScheduled = DateTime.Now;

                List<MSTeamOperation> remainingRemoveOperations = removeRequests.Where(operation => !operation.IsCompleted && !operation.IsDeleted).Skip(1).ToList();

                foreach (MSTeamOperation operation in remainingRemoveOperations)
                {
                    operation.IsDeleted = true;
                }
            }

            List<MSTeamOperation> remainingAddOperations = addRequests.Where(operation => !operation.IsCompleted && !operation.IsDeleted).ToList();

            foreach (MSTeamOperation operation in remainingAddOperations)
            {
                operation.IsDeleted = true;
            }
        }
        else
        {
            List<MSTeamOperation> removeOperations = removeRequests.Where(operation => !operation.IsCompleted && !operation.IsDeleted).ToList();

            foreach (MSTeamOperation operation in removeOperations)
            {
                operation.IsDeleted = true;
            }

            List<MSTeamOperation> addOperations = addRequests.Where(operation => !operation.IsCompleted && !operation.IsDeleted).ToList();

            foreach (MSTeamOperation operation in addOperations)
            {
                operation.IsDeleted = true;
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
