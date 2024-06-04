namespace Constellation.Application.Faculties.Events.FacultyMemberRemovedDomainEvent;

using Abstractions.Messaging;
using Constellation.Application.Enums;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Core.Models.Faculties;
using Core.Models.Faculties.Events;
using Core.Models.Faculties.Repositories;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateStudentTeamMembershipLevel
    : IDomainEventHandler<FacultyMemberRemovedDomainEvent>
{
    private readonly IFacultyRepository _facultyRepository;
    private readonly IMSTeamOperationsRepository _operationsRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateStudentTeamMembershipLevel(
        IFacultyRepository facultyRepository,
        IMSTeamOperationsRepository operationsRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork)
    {
        _facultyRepository = facultyRepository;
        _operationsRepository = operationsRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(FacultyMemberRemovedDomainEvent notification, CancellationToken cancellationToken)
    {
        Faculty faculty = await _facultyRepository.GetById(notification.FacultyId, cancellationToken);
        FacultyMembership membership = faculty
            .Members
            .FirstOrDefault(entry => entry.Id == notification.FacultyMembershipId);

        if (membership is null)
            return;

        if (!faculty.Name.Contains("Administration") &&
            !faculty.Name.Contains("Executive") &&
            !faculty.Name.Contains("Support"))
            return;

        // Create Operation
        TeacherEmployedMSTeamOperation studentTeamOperation = new()
        {
            StaffId = membership.StaffId,
            TeamName = MicrosoftTeam.Students,
            Action = MSTeamOperationAction.Demote,
            DateScheduled = _dateTime.Now,
            PermissionLevel = MSTeamOperationPermissionLevel.Member
        };

        _operationsRepository.Insert(studentTeamOperation);
        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
