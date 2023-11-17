namespace Constellation.Application.Faculties.Events.FacultyMemberAddedDomainEvent;

using Abstractions.Messaging;
using Constellation.Application.Enums;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Faculty.Repositories;
using Core.Abstractions.Clock;
using Core.Models.Faculty;
using Core.Models.Faculty.Events;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddToAdobeConnectFacultyGroup 
    : IDomainEventHandler<FacultyMemberAddedDomainEvent>
{
    private readonly IFacultyRepository _facultyRepository;
    private readonly IAdobeConnectOperationsRepository _operationsRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;

    public AddToAdobeConnectFacultyGroup(
        IFacultyRepository facultyRepository,
        IAdobeConnectOperationsRepository operationsRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork)
    {
        _facultyRepository = facultyRepository;
        _operationsRepository = operationsRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
    }
    
    public async Task Handle(FacultyMemberAddedDomainEvent notification, CancellationToken cancellationToken)
    {
        Faculty faculty = await _facultyRepository.GetById(notification.FacultyId, cancellationToken);
        FacultyMembership membership = faculty
            .Members
            .FirstOrDefault(entry => entry.Id == notification.FacultyMembershipId);

        if (membership is null)
            return;

        bool success = Enum.TryParse(faculty.Name, true, out AdobeConnectGroup group);

        if (!success)
            return;

        TeacherAdobeConnectGroupOperation operation = new()
        {
            GroupSco = ((int)group).ToString(),
            GroupName = group.ToString(),
            TeacherId = membership.StaffId,
            Action = AdobeConnectOperationAction.Add,
            DateScheduled = _dateTime.Now
        };

        _operationsRepository.Insert(operation);
        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
