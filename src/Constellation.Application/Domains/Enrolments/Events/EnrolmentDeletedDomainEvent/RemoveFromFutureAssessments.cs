namespace Constellation.Application.Domains.Enrolments.Events.EnrolmentDeletedDomainEvent;

using Abstractions.Messaging;
using Constellation.Core.Models.Enrolments.Errors;
using Constellation.Core.Models.Enrolments.Repositories;
using Constellation.Core.Models.Students.Errors;
using Core.Models.Assessments;
using Core.Models.Assessments.Repositories;
using Core.Models.Enrolments;
using Core.Models.Enrolments.Events;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;

internal sealed class RemoveFromFutureAssessments
: IDomainEventHandler<EnrolmentDeletedDomainEvent>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveFromFutureAssessments(
        IAssessmentRepository assessmentRepository,
        IEnrolmentRepository enrolmentRepository,
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _enrolmentRepository = enrolmentRepository;
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<EnrolmentDeletedDomainEvent>();
    }

    public async Task Handle(EnrolmentDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        Enrolment? enrolment = await _enrolmentRepository.GetById(notification.EnrolmentId, cancellationToken);

        if (enrolment is null)
        {
            _logger
                .ForContext(nameof(EnrolmentDeletedDomainEvent), notification, true)
                .ForContext(nameof(Error), EnrolmentErrors.NotFound(notification.EnrolmentId), true)
                .Error("Failed to complete the event handler");

            return;
        }

        Student? student = await _studentRepository.GetById(enrolment.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(EnrolmentDeletedDomainEvent), notification, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(enrolment.StudentId))
                .Error("Failed to complete the event handler");

            return;
        }

        List<Assessment> assessments = await _assessmentRepository.GetAssessmentsForStudent(student.Id, cancellationToken);

        foreach (Assessment assessment in assessments)
        {
            if (assessment.DueDate > DateTime.UtcNow)
                assessment.RemoveStudent(student.Id);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
