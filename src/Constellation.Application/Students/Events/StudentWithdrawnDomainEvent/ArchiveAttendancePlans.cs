namespace Constellation.Application.Students.Events.StudentWithdrawnDomainEvent;

using Abstractions.Messaging;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Repositories;
using Core.Abstractions.Services;
using Core.Models.Attendance;
using Core.Models.Attendance.Repositories;
using Core.Models.Students.Events;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ArchiveAttendancePlans
    : IDomainEventHandler<StudentWithdrawnDomainEvent>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IAttendancePlanRepository _planRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public ArchiveAttendancePlans(
        IStudentRepository studentRepository,
        IAttendancePlanRepository planRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _planRepository = planRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<StudentWithdrawnDomainEvent>();
    }

    public async Task Handle(StudentWithdrawnDomainEvent notification, CancellationToken cancellationToken)
    {
        Student student = await _studentRepository.GetById(notification.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(StudentWithdrawnDomainEvent), notification, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(notification.StudentId), true)
                .Warning("Failed to archive attendance plans for withdrawn Student");

            return;
        }

        List<AttendancePlan> attendancePlans = await _planRepository.GetForStudent(student.Id, cancellationToken);

        foreach (AttendancePlan plan in attendancePlans)
        {
            Result archival = plan.ArchivePlan();

            if (archival.IsFailure)
            {
                _logger
                    .ForContext(nameof(StudentWithdrawnDomainEvent), notification, true)
                    .ForContext(nameof(Error), archival.Error, true)
                    .ForContext(nameof(AttendancePlan), plan, true)
                    .Warning("Failed to archive attendance plans for withdrawn Student");
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}