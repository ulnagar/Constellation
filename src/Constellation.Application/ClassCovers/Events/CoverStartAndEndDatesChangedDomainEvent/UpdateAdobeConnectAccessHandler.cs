namespace Constellation.Application.ClassCovers.Events.CoverStartAndEndDatesChangedDomainEvent;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Core.DomainEvents;
using Core.Models.Covers;
using Core.ValueObjects;
using Interfaces.Repositories;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateAdobeConnectAccessHandler
    : IDomainEventHandler<CoverStartAndEndDatesChangedDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAdobeConnectOperationsRepository _operationsRepository;
    private readonly IClassCoverRepository _classCoverRepository;
    private readonly ILogger _logger;

    public UpdateAdobeConnectAccessHandler(
        IUnitOfWork unitOfWork,
        IAdobeConnectOperationsRepository operationsRepository,
        IClassCoverRepository classCoverRepository,
        ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _operationsRepository = operationsRepository;
        _classCoverRepository = classCoverRepository;
        _logger = logger.ForContext<CoverStartAndEndDatesChangedDomainEvent>();
    }

    public async Task Handle(CoverStartAndEndDatesChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        ClassCover cover = await _classCoverRepository.GetById(notification.CoverId, cancellationToken);

        if (cover is null)
        {
            _logger.Error("{action}: Could not find cover with Id {id} in database", nameof(UpdateAdobeConnectAccessHandler), notification.CoverId);

            return;
        }

        List<AdobeConnectOperation> existingRequests = await _operationsRepository
            .GetByCoverId(notification.CoverId, cancellationToken);

        if (existingRequests is null)
        {
            _logger.Warning("{action}: Could not find operations for cover with Id {id} in database", nameof(UpdateAdobeConnectAccessHandler), notification.CoverId);

            return;
        }

        IEnumerable<IGrouping<string, AdobeConnectOperation>> requestsByRoom = existingRequests
            .Where(operation => !operation.IsDeleted)
            .GroupBy(operation => operation.ScoId);

        foreach (IGrouping<string, AdobeConnectOperation> room in requestsByRoom)
        {
            List<AdobeConnectOperation> addRequests = room
                .Where(operation =>
                    operation.Action == AdobeConnectOperationAction.Add)
                .ToList();

            List<AdobeConnectOperation> removeRequests = room
                .Where(operation =>
                    operation.Action == AdobeConnectOperationAction.Remove)
                .ToList();

            // If access has not been granted, change the operation
            // If it has, determine whether there is a big enough difference to remove access and then grant later or just leave it
            AdobeConnectOperation alreadyGranted = addRequests.FirstOrDefault(operation => operation.IsCompleted);

            if (alreadyGranted is null)
            {
                foreach (AdobeConnectOperation request in addRequests)
                {
                    request.DateScheduled = notification.NewStartDate.ToDateTime(TimeOnly.MinValue).AddDays(-1);
                }
            }
            else
            {
                DateTime previousActionDate = alreadyGranted.DateScheduled;
                DateTime newActionDate = notification.NewStartDate.ToDateTime(TimeOnly.MinValue).AddDays(-1);

                alreadyGranted.Delete();

                if (newActionDate.Subtract(previousActionDate) > TimeSpan.FromDays(2))
                {
                    // Remove access, then create new add operation
                    if (cover.TeacherType == CoverTeacherType.Casual)
                    {
                        CasualAdobeConnectOperation removeEarlyOperation = new()
                        {
                            ScoId = room.Key,
                            CasualId = Guid.Parse(cover.TeacherId),
                            Action = AdobeConnectOperationAction.Remove,
                            DateScheduled = DateTime.Now,
                            CoverId = Guid.Empty
                        };

                        _operationsRepository.Insert(removeEarlyOperation);
                    }
                    else
                    {
                        TeacherAdobeConnectOperation removeEarlyOperation = new()
                        {
                            ScoId = room.Key,
                            StaffId = cover.TeacherId,
                            Action = AdobeConnectOperationAction.Remove,
                            DateScheduled = DateTime.Now,
                            CoverId = Guid.Empty
                        };

                        _operationsRepository.Insert(removeEarlyOperation);
                    }
                }

                if (cover.TeacherType == CoverTeacherType.Casual)
                {
                    CasualAdobeConnectOperation addTimelyOperation = new()
                    {
                        ScoId = room.Key,
                        CasualId = Guid.Parse(cover.TeacherId),
                        Action = AdobeConnectOperationAction.Add,
                        DateScheduled = newActionDate,
                        CoverId = cover.Id.Value
                    };

                    _operationsRepository.Insert(addTimelyOperation);
                }
                else
                {
                    TeacherAdobeConnectOperation addTimelyOperation = new()
                    {
                        ScoId = room.Key,
                        StaffId = cover.TeacherId,
                        Action = AdobeConnectOperationAction.Add,
                        DateScheduled = newActionDate,
                        CoverId = cover.Id.Value
                    };

                    _operationsRepository.Insert(addTimelyOperation);
                }
            }

            // Process removal
            AdobeConnectOperation alreadyRemoved = removeRequests.FirstOrDefault(operation => operation.IsCompleted);

            if (alreadyRemoved is null)
            {
                foreach (AdobeConnectOperation request in removeRequests)
                {
                    request.DateScheduled = notification.NewEndDate.ToDateTime(TimeOnly.MinValue).AddDays(1);
                }
            }
            else
            {
                DateTime previousActionDate = alreadyRemoved.DateScheduled;
                DateTime newActionDate = notification.NewEndDate.ToDateTime(TimeOnly.MinValue).AddDays(1);

                alreadyRemoved.Delete();

                if (newActionDate > previousActionDate)
                {
                    // Remove access, then create new add operation
                    if (cover.TeacherType == CoverTeacherType.Casual)
                    {
                        CasualAdobeConnectOperation reAddOperation = new()
                        {
                            ScoId = room.Key,
                            CasualId = Guid.Parse(cover.TeacherId),
                            Action = AdobeConnectOperationAction.Add,
                            DateScheduled = DateTime.Now,
                            CoverId = Guid.Empty
                        };

                        _operationsRepository.Insert(reAddOperation);
                    }
                    else
                    {
                        TeacherAdobeConnectOperation reAddOperation = new()
                        {
                            ScoId = room.Key,
                            StaffId = cover.TeacherId,
                            Action = AdobeConnectOperationAction.Add,
                            DateScheduled = DateTime.Now,
                            CoverId = Guid.Empty
                        };

                        _operationsRepository.Insert(reAddOperation);
                    }
                }

                if (cover.TeacherType == CoverTeacherType.Casual)
                {
                    CasualAdobeConnectOperation removeTimelyOperation = new()
                    {
                        ScoId = room.Key,
                        CasualId = Guid.Parse(cover.TeacherId),
                        Action = AdobeConnectOperationAction.Remove,
                        DateScheduled = newActionDate,
                        CoverId = cover.Id.Value
                    };

                    _operationsRepository.Insert(removeTimelyOperation);
                }
                else
                {
                    TeacherAdobeConnectOperation removeTimelyOperation = new()
                    {
                        ScoId = room.Key,
                        StaffId = cover.TeacherId,
                        Action = AdobeConnectOperationAction.Remove,
                        DateScheduled = newActionDate,
                        CoverId = cover.Id.Value
                    };

                    _operationsRepository.Insert(removeTimelyOperation);
                }
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
