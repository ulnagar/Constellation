namespace Constellation.Application.ClassCovers.Events;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.DomainEvents;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.ValueObjects;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CoverCreatedDomainEvent_CreateMicrosoftTeamsAccessHandler
    : IDomainEventHandler<CoverCreatedDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClassCoverRepository _classCoverRepository;
    private readonly IMSTeamOperationsRepository _operationsRepository;
    private readonly ILogger _logger;

    public CoverCreatedDomainEvent_CreateMicrosoftTeamsAccessHandler(
        IUnitOfWork unitOfWork,
        IClassCoverRepository classCoverRepository,
        IMSTeamOperationsRepository operationsRepository,
        Serilog.ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _classCoverRepository = classCoverRepository;
        _operationsRepository = operationsRepository;
        _logger = logger.ForContext<CoverCreatedDomainEvent>();
    }

    public async Task Handle(CoverCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var cover = await _classCoverRepository.GetById(notification.CoverId, cancellationToken);

        if (cover is null)
        {
            _logger.Error("{action}: Could not find cover with Id {id} in database", nameof(CoverCreatedDomainEvent_CreateMicrosoftTeamsAccessHandler), notification.CoverId);

            return;
        }

        if (cover.TeacherType == CoverTeacherType.Casual)
        {
            var addOperation = new CasualMSTeamOperation
            {
                OfferingId = cover.OfferingId,
                CasualId = Guid.Parse(cover.TeacherId),
                CoverId = cover.Id.Value,
                Action = MSTeamOperationAction.Add,
                PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                DateScheduled = cover.StartDate.ToDateTime(TimeOnly.MinValue).AddDays(-1)
            };

            _operationsRepository.Insert(addOperation);

            var removeOperation = new CasualMSTeamOperation
            {
                OfferingId = cover.OfferingId,
                CasualId = Guid.Parse(cover.TeacherId),
                CoverId = cover.Id.Value,
                Action = MSTeamOperationAction.Remove,
                PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                DateScheduled = cover.EndDate.ToDateTime(TimeOnly.MinValue).AddDays(1)
            };

            _operationsRepository.Insert(removeOperation);
        } 
        else
        {
            var addOperation = new TeacherMSTeamOperation
            {
                OfferingId = cover.OfferingId,
                StaffId = cover.TeacherId,
                CoverId = cover.Id.Value,
                Action = MSTeamOperationAction.Add,
                PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                DateScheduled = cover.StartDate.ToDateTime(TimeOnly.MinValue).AddDays(-1)
            };

            _operationsRepository.Insert(addOperation);

            var removeOperation = new TeacherMSTeamOperation
            {
                OfferingId = cover.OfferingId,
                StaffId = cover.TeacherId,
                CoverId = cover.Id.Value,
                Action = MSTeamOperationAction.Remove,
                PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                DateScheduled = cover.EndDate.ToDateTime(TimeOnly.MinValue).AddDays(1)
            };

            _operationsRepository.Insert(removeOperation);
        }

        // Add the Casual Coordinators to the Team as well
        var cathyAddOperation = new TeacherMSTeamOperation
        {
            OfferingId = cover.OfferingId,
            StaffId = "1030937",
            Action = MSTeamOperationAction.Add,
            PermissionLevel = MSTeamOperationPermissionLevel.Owner,
            DateScheduled = cover.StartDate.ToDateTime(TimeOnly.MinValue).AddDays(-1),
            CoverId = cover.Id.Value
        };

        _operationsRepository.Insert(cathyAddOperation);

        var cathyRemoveOperation = new TeacherMSTeamOperation
        {
            OfferingId = cover.OfferingId,
            StaffId = "1030937",
            Action = MSTeamOperationAction.Remove,
            PermissionLevel = MSTeamOperationPermissionLevel.Owner,
            DateScheduled = cover.EndDate.ToDateTime(TimeOnly.MinValue).AddDays(1),
            CoverId = cover.Id.Value
        };

        _operationsRepository.Insert(cathyRemoveOperation);

        var karenAddOperation = new TeacherMSTeamOperation
        {
            OfferingId = cover.OfferingId,
            StaffId = "1112830",
            Action = MSTeamOperationAction.Add,
            PermissionLevel = MSTeamOperationPermissionLevel.Owner,
            DateScheduled = cover.StartDate.ToDateTime(TimeOnly.MinValue).AddDays(-1),
            CoverId = cover.Id.Value
        };

        _operationsRepository.Insert(karenAddOperation);

        var karenRemoveOperation = new TeacherMSTeamOperation
        {
            OfferingId = cover.OfferingId,
            StaffId = "1112830",
            Action = MSTeamOperationAction.Remove,
            PermissionLevel = MSTeamOperationPermissionLevel.Owner,
            DateScheduled = cover.EndDate.ToDateTime(TimeOnly.MinValue).AddDays(1),
            CoverId = cover.Id.Value
        };

        _operationsRepository.Insert(karenRemoveOperation);

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
