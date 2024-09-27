namespace Constellation.Application.Students.Events.StudentMovedSchoolsDomainEvent;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Events;
using Core.Models.Students.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemovePreviousSchoolEnrolment
    : IDomainEventHandler<StudentMovedSchoolsDomainEvent>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public RemovePreviousSchoolEnrolment(
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
        _logger = logger
            .ForContext<StudentMovedSchoolsDomainEvent>();
    }

    public async Task Handle(StudentMovedSchoolsDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger
            .ForContext(nameof(StudentMovedSchoolsDomainEvent), notification, true)
            .Information("Updating school enrolments for student");

        Student student = await _studentRepository.GetById(notification.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(StudentMovedSchoolsDomainEvent), notification, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(notification.StudentId), true)
                .Warning("Failed to process Student school change");

            return;
        }

        // Find new enrolment.
        // Check if the new enrolment is still valid. E.g. has it been deleted?
        // If it is still valid, find any other active enrolments and delete them.

        SchoolEnrolment newEnrolment = student.SchoolEnrolments
            .FirstOrDefault(entry =>
                !entry.IsDeleted &&
                entry.SchoolCode == notification.CurrentSchoolCode &&
                entry.StartDate == _dateTime.Today);

        if (newEnrolment is null)
        {
            _logger
                .ForContext(nameof(StudentMovedSchoolsDomainEvent), notification, true)
                .ForContext(nameof(Error), SchoolEnrolmentErrors.NotFound, true)
                .Error("Failed to process Student school change");

            return;
        }

        if (newEnrolment.IsDeleted)
        {
            _logger
                .ForContext(nameof(StudentMovedSchoolsDomainEvent), notification, true)
                .ForContext(nameof(Error), SchoolEnrolmentErrors.RecordDeleted, true)
                .Error("Failed to process Student school change");

            return;
        }

        List<SchoolEnrolment> oldEnrolments = student.SchoolEnrolments
            .Where(enrolment =>
                !enrolment.IsDeleted &&
                enrolment.StartDate < _dateTime.Today)
            .ToList();

        foreach (SchoolEnrolment enrolment in oldEnrolments)
        {
            student.RemoveSchoolEnrolment(enrolment, _dateTime);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        _logger
            .ForContext(nameof(StudentMovedSchoolsDomainEvent), notification, true)
            .Information("Successfully updated school enrolments for student");
    }
}