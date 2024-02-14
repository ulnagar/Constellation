namespace Constellation.Application.ClassCovers.Events.CoverCreatedDomainEvent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.DomainEvents;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.ValueObjects;
using Core.Models.Covers;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateAdobeConnectAccessHandler
    : IDomainEventHandler<CoverCreatedDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAdobeConnectOperationsRepository _operationsRepository;
    private readonly IClassCoverRepository _classCoverRepository;
    private readonly IAdobeConnectRoomRepository _roomRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;

    public CreateAdobeConnectAccessHandler(
        IUnitOfWork unitOfWork,
        IAdobeConnectOperationsRepository operationsRepository,
        IClassCoverRepository classCoverRepository,
        IAdobeConnectRoomRepository roomRepository,
        UserManager<AppUser> userManager,
        ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _operationsRepository = operationsRepository;
        _classCoverRepository = classCoverRepository;
        _roomRepository = roomRepository;
        _userManager = userManager;
        _logger = logger.ForContext<CoverCreatedDomainEvent>();
    }

    public async Task Handle(CoverCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        ClassCover cover = await _classCoverRepository.GetById(notification.CoverId, cancellationToken);

        if (cover is null)
        {
            _logger.Error("{action}: Could not find cover with Id {id} in database", nameof(CreateAdobeConnectAccessHandler), notification.CoverId);

            return;
        }

        List<AdobeConnectRoom> rooms = await _roomRepository.GetByOfferingId(cover.OfferingId, cancellationToken);

        foreach (AdobeConnectRoom room in rooms)
        {
            if (cover.TeacherType == CoverTeacherType.Casual)
            {
                CasualAdobeConnectOperation addOperation = new()
                {
                    ScoId = room.ScoId,
                    CasualId = Guid.Parse(cover.TeacherId),
                    Action = AdobeConnectOperationAction.Add,
                    DateScheduled = cover.StartDate.ToDateTime(TimeOnly.MinValue).AddDays(-1),
                    CoverId = cover.Id.Value
                };

                _operationsRepository.Insert(addOperation);

                CasualAdobeConnectOperation removeOperation = new()
                {
                    ScoId = room.ScoId,
                    CasualId = Guid.Parse(cover.TeacherId),
                    Action = AdobeConnectOperationAction.Remove,
                    DateScheduled = cover.EndDate.ToDateTime(TimeOnly.MinValue).AddDays(1),
                    CoverId = cover.Id.Value
                };

                _operationsRepository.Insert(removeOperation);
            }
            else
            {
                TeacherAdobeConnectOperation addOperation = new()
                {
                    ScoId = room.ScoId,
                    StaffId = cover.TeacherId,
                    Action = AdobeConnectOperationAction.Add,
                    DateScheduled = cover.StartDate.ToDateTime(TimeOnly.MinValue).AddDays(-1),
                    CoverId = cover.Id.Value
                };

                _operationsRepository.Insert(addOperation);

                TeacherAdobeConnectOperation removeOperation = new()
                {
                    ScoId = room.ScoId,
                    StaffId = cover.TeacherId,
                    Action = AdobeConnectOperationAction.Remove,
                    DateScheduled = cover.EndDate.ToDateTime(TimeOnly.MinValue).AddDays(1),
                    CoverId = cover.Id.Value
                };

                _operationsRepository.Insert(removeOperation);
            }

            // Cover administrators
            IList<AppUser> additionalRecipients = await _userManager.GetUsersInRoleAsync(AuthRoles.CoverRecipient);

            foreach (AppUser coverAdmin in additionalRecipients)
            {
                if (!coverAdmin.IsStaffMember)
                    continue;

                TeacherAdobeConnectOperation addOperation = new()
                {
                    ScoId = room.ScoId,
                    StaffId = coverAdmin.StaffId,
                    Action = AdobeConnectOperationAction.Remove,
                    DateScheduled = cover.StartDate.ToDateTime(TimeOnly.MinValue).AddDays(-1),
                    CoverId = cover.Id.Value
                };

                _operationsRepository.Insert(addOperation);

                TeacherAdobeConnectOperation removeOperation = new()
                {
                    ScoId = room.ScoId,
                    StaffId = coverAdmin.StaffId,
                    Action = AdobeConnectOperationAction.Remove,
                    DateScheduled = cover.EndDate.ToDateTime(TimeOnly.MinValue).AddDays(1),
                    CoverId = cover.Id.Value
                };

                _operationsRepository.Insert(removeOperation);
            }

            await _unitOfWork.CompleteAsync(cancellationToken);
        }
    }
}