namespace Constellation.Application.Faculties.Events.FacultyMemberRemovedDomainEvent;

using Abstractions.Messaging;
using Constellation.Application.Enums;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Core.Models.Faculties;
using Core.Models.Faculties.Events;
using Core.Models.Faculties.Repositories;
using Core.Models.Faculties.ValueObjects;
using Core.Models.Operations;
using Core.Models.Operations.Enums;
using Core.Models.Operations.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Repositories;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateStudentTeamMembershipLevel
    : IDomainEventHandler<FacultyMemberRemovedDomainEvent>
{
    private readonly IFacultyRepository _facultyRepository;
    private readonly ITeamOperationRepository _operationsRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateStudentTeamMembershipLevel(
        IFacultyRepository facultyRepository,
        ITeamOperationRepository operationsRepository,
        IStaffRepository staffRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork)
    {
        _facultyRepository = facultyRepository;
        _operationsRepository = operationsRepository;
        _staffRepository = staffRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(FacultyMemberRemovedDomainEvent notification, CancellationToken cancellationToken)
    {
        // 2026-01-23 : Remove below
        //      Code implies that all Faculty Members are Owners of the Students Team,
        //      however it should only be Faculty Managers, and this is managed elsewhere.

        //Faculty? faculty = await _facultyRepository.GetById(notification.FacultyId, cancellationToken);

        //if (faculty is null)
        //{
        //    return;
        //}

        //FacultyMembership? membership = faculty
        //    .Members
        //    .FirstOrDefault(entry => entry.Id == notification.FacultyMembershipId);

        //if (membership is null)
        //    return;

        //if (!faculty.Name.Contains("Administration") &&
        //    !faculty.Name.Contains("Executive") &&
        //    !faculty.Name.Contains("Support"))
        //    return;

        //if (membership.Role != FacultyMembershipRole.Manager)
        //    return;

        //StaffMember? staffMember = await _staffRepository.GetById(membership.StaffId, cancellationToken);

        //if (staffMember is null)
        //    return;

        //// Create Operation
        //ModifyTeamMembershipTeamOperation studentTeamOperation = new(
        //    MicrosoftTeam.StudentsTeamId,
        //    staffMember.EmailAddress,
        //    TeamAction.AddMember);

        //_operationsRepository.Insert(studentTeamOperation);
        //await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
