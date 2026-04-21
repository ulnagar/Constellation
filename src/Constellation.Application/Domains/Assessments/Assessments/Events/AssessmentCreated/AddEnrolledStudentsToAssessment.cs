namespace Constellation.Application.Domains.Assessments.Assessments.Events.AssessmentCreated;

using Abstractions.Messaging;
using Core.Models.Assessments;
using Core.Models.Assessments.Errors;
using Core.Models.Assessments.Events;
using Core.Models.Assessments.Repositories;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Newtonsoft.Json.Serialization;
using Serilog;

internal sealed class AddEnrolledStudentsToAssessment
: IDomainEventHandler<AssessmentCreatedDomainEvent>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddEnrolledStudentsToAssessment(
        IAssessmentRepository assessmentRepository,
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<AssessmentCreatedDomainEvent>();
    }

    public async Task Handle(AssessmentCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        Assessment? assessment = await _assessmentRepository.GetAssessmentById(notification.AssessmentId, cancellationToken);

        if (assessment is null)
        {
            _logger
                .ForContext(nameof(AssessmentCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), AssessmentErrors.NotFound(notification.AssessmentId), true)
                .Warning("Failed to add enrolled students to new Assessment");

            return;
        }

        List<Student> students = await _studentRepository.GetCurrentEnrolmentsForCourse(assessment.CourseId, cancellationToken);
        
        foreach (Student student in students)
        {
            List<Provision> provisions = await _assessmentRepository.GetCurrentProvisionsForStudent(student.Id, cancellationToken);
            
            assessment.AddStudent(student, provisions);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
