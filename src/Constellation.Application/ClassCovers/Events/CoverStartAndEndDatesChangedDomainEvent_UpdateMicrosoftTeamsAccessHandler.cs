using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.DomainEvents;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.ValueObjects;
using Serilog;
using System.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.ClassCovers.Events;

internal sealed class CoverStartAndEndDatesChangedDomainEvent_UpdateMicrosoftTeamsAccessHandler
    : IDomainEventHandler<CoverStartAndEndDatesChangedDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMSTeamOperationsRepository _operationsRepository;
    private readonly IClassCoverRepository _classCoverRepository;
    private readonly ILogger _logger;

    public CoverStartAndEndDatesChangedDomainEvent_UpdateMicrosoftTeamsAccessHandler(
        IUnitOfWork unitOfWork,
        IMSTeamOperationsRepository operationsRepository,
        IClassCoverRepository classCoverRepository,
        Serilog.ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _operationsRepository = operationsRepository;
        _classCoverRepository = classCoverRepository;
        _logger = logger.ForContext<CoverStartAndEndDatesChangedDomainEvent>();
    }

    public async Task Handle(CoverStartAndEndDatesChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        var cover = await _classCoverRepository.GetById(notification.CoverId, cancellationToken);

        if (cover is null)
        {
            _logger.Warning("{action}: Could not find cover with Id {id} in database", nameof(CoverEndDateChangedDomainEvent_UpdateMicrosoftTeamsAccessHandler), notification.CoverId);

            return;
        }

        var existingRequests = await _operationsRepository
            .GetByCoverId(notification.CoverId, cancellationToken);

        var addRequests = existingRequests
            .Where(operation => 
                operation.Action == MSTeamOperationAction.Add)
            .ToList();

        var removeRequests = existingRequests
            .Where(operation =>
                operation.Action == MSTeamOperationAction.Remove)
            .ToList();

        // If access has not been granted, change the operation
        // If it has, determine whether there is a big enough difference to remove access and then grant later or just leave it
        var alreadyGranted = addRequests.Any(operation => operation.IsCompleted);

        if (!alreadyGranted)
        {
            foreach (var request in addRequests)
            {
                request.DateScheduled = notification.NewStartDate.ToDateTime(TimeOnly.MinValue).AddDays(-1);
            }
        }
        else
        {
            var previousActionDate = notification.PreviousStartDate.ToDateTime(TimeOnly.MinValue).AddDays(-1);
            var newActionDate = notification.NewStartDate.ToDateTime(TimeOnly.MinValue).AddDays(-1);

            if (newActionDate.Subtract(previousActionDate) > TimeSpan.FromDays(2))
            {
                // Remove access, then create new add operation
                if (cover.TeacherType == CoverTeacherType.Casual)
                {
                    var removeEarlyOperation = new CasualMSTeamOperation
                    {
                        OfferingId = cover.OfferingId,
                        CasualId = int.Parse(cover.TeacherId),
                        CoverId = cover.Id,
                        Action = MSTeamOperationAction.Remove,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = DateTime.Today
                    };

                    _operationsRepository.Insert(removeEarlyOperation);
                }
                else
                {
                    var removeEarlyOperation = new TeacherMSTeamOperation
                    {
                        OfferingId = cover.OfferingId,
                        StaffId = cover.TeacherId,
                        CoverId = cover.Id,
                        Action = MSTeamOperationAction.Remove,
                        PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                        DateScheduled = DateTime.Today
                    };

                    _operationsRepository.Insert(removeEarlyOperation);
                }

                var cathyRemoveOperation = new TeacherMSTeamOperation
                {
                    OfferingId = cover.OfferingId,
                    StaffId = "1030937",
                    Action = MSTeamOperationAction.Remove,
                    PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                    DateScheduled = DateTime.Today,
                    CoverId = cover.Id
                };

                _operationsRepository.Insert(cathyRemoveOperation);

                var karenRemoveOperation = new TeacherMSTeamOperation
                {
                    OfferingId = cover.OfferingId,
                    StaffId = "1112830",
                    Action = MSTeamOperationAction.Remove,
                    PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                    DateScheduled = DateTime.Today,
                    CoverId = cover.Id
                };

                _operationsRepository.Insert(karenRemoveOperation);
            }

            // Access should be granted within a day or two, but the audit may still remove you
            // So, lets create new (hopefully redundant) add operations to ensure access is granted on the day

            if (cover.TeacherType == CoverTeacherType.Casual)
            {
                var addTimelyOperation = new CasualMSTeamOperation
                {
                    OfferingId = cover.OfferingId,
                    CasualId = int.Parse(cover.TeacherId),
                    CoverId = cover.Id,
                    Action = MSTeamOperationAction.Add,
                    PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                    DateScheduled = newActionDate
                };

                _operationsRepository.Insert(addTimelyOperation);
            }
            else
            {
                var addTimelyOperation = new TeacherMSTeamOperation
                {
                    OfferingId = cover.OfferingId,
                    StaffId = cover.TeacherId,
                    CoverId = cover.Id,
                    Action = MSTeamOperationAction.Add,
                    PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                    DateScheduled = newActionDate
                };

                _operationsRepository.Insert(addTimelyOperation);
            }

            var cathyAddOperation = new TeacherMSTeamOperation
            {
                OfferingId = cover.OfferingId,
                StaffId = "1030937",
                Action = MSTeamOperationAction.Add,
                PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                DateScheduled = newActionDate,
                CoverId = cover.Id
            };

            _operationsRepository.Insert(cathyAddOperation);

            var karenAddOperation = new TeacherMSTeamOperation
            {
                OfferingId = cover.OfferingId,
                StaffId = "1112830",
                Action = MSTeamOperationAction.Add,
                PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                DateScheduled = newActionDate,
                CoverId = cover.Id
            };

            _operationsRepository.Insert(karenAddOperation);
        }
        
        // Set the first unactioned remove request to process asap
        var removeOperations = removeRequests.Where(operation => !operation.IsCompleted && !operation.IsDeleted).ToList();

        if (removeOperations.Count == 0)
        {
            // Create new operation to remove access
            if (cover.TeacherType == CoverTeacherType.Casual)
            {
                var removeOperation = new CasualMSTeamOperation
                {
                    OfferingId = cover.OfferingId,
                    CasualId = int.Parse(cover.TeacherId),
                    CoverId = cover.Id,
                    Action = MSTeamOperationAction.Remove,
                    PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                    DateScheduled = notification.NewEndDate.ToDateTime(TimeOnly.MinValue).AddDays(1)
                };

                _operationsRepository.Insert(removeOperation);
            }
            else
            {
                var removeOperation = new TeacherMSTeamOperation
                {
                    OfferingId = cover.OfferingId,
                    StaffId = cover.TeacherId,
                    CoverId = cover.Id,
                    Action = MSTeamOperationAction.Remove,
                    PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                    DateScheduled = notification.NewEndDate.ToDateTime(TimeOnly.MinValue).AddDays(1)
                };

                _operationsRepository.Insert(removeOperation);
            }

            var cathyRemoveOperation = new TeacherMSTeamOperation
            {
                OfferingId = cover.OfferingId,
                StaffId = "1030937",
                Action = MSTeamOperationAction.Remove,
                PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                DateScheduled = notification.NewEndDate.ToDateTime(TimeOnly.MinValue).AddDays(1),
                CoverId = cover.Id
            };

            _operationsRepository.Insert(cathyRemoveOperation);

            var karenRemoveOperation = new TeacherMSTeamOperation
            {
                OfferingId = cover.OfferingId,
                StaffId = "1112830",
                Action = MSTeamOperationAction.Remove,
                PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                DateScheduled = notification.NewEndDate.ToDateTime(TimeOnly.MinValue).AddDays(1),
                CoverId = cover.Id
            };

            _operationsRepository.Insert(karenRemoveOperation);
        }
        else
        {
            foreach (var operation in removeOperations)
            {
                operation.DateScheduled = notification.NewEndDate.ToDateTime(TimeOnly.MinValue).AddDays(1);
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
