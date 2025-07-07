namespace Constellation.Application.Domains.StaffMembers.Events.StaffMemberResignedDomainEvent;

using Abstractions.Messaging;
using Constellation.Core.Models.Faculties;
using Constellation.Core.Models.Faculties.Repositories;
using Core.Models.StaffMembers.Events;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveFacultyMemberships
:IDomainEventHandler<StaffMemberResignedDomainEvent>
{
    private readonly IFacultyRepository _facultyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveFacultyMemberships(
        IFacultyRepository facultyRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _facultyRepository = facultyRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(StaffMemberResignedDomainEvent notification, CancellationToken cancellationToken)
    {
        List<Faculty> faculties = await _facultyRepository.GetCurrentForStaffMember(notification.StaffId, cancellationToken);
        
        foreach (Faculty faculty in faculties)
        {
            faculty.RemoveMember(notification.StaffId);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
