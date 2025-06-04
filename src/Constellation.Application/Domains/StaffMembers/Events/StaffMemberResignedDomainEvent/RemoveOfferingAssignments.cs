namespace Constellation.Application.Domains.StaffMembers.Events.StaffMemberResignedDomainEvent;

using Abstractions.Messaging;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Core.Models.StaffMembers.Events;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveOfferingAssignments
: IDomainEventHandler<StaffMemberResignedDomainEvent>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveOfferingAssignments(
        IOfferingRepository offeringRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(StaffMemberResignedDomainEvent notification, CancellationToken cancellationToken)
    {
        List<Offering> offerings = await _offeringRepository.GetAllTypesActiveForTeacher(notification.StaffId, cancellationToken);

        foreach (Offering offering in offerings)
        {
            List<TeacherAssignment> assignments = offering
                .Teachers
                .Where(assignment =>
                    assignment.StaffId == notification.StaffId &&
                    !assignment.IsDeleted)
                .ToList();

            foreach (TeacherAssignment assignment in assignments)
                offering.RemoveTeacher(notification.StaffId, assignment.Type);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
