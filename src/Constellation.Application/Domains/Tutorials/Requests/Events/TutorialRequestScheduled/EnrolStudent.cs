namespace Constellation.Application.Domains.Tutorials.Requests.Events.TutorialRequestScheduled;

using Abstractions.Messaging;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Tutorials.Errors;
using Constellation.Core.Models.Tutorials.Identifiers;
using Constellation.Core.Shared;
using Core.Models.Enrolments;
using Core.Models.Enrolments.Repositories;
using Core.Models.Students.Repositories;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Events;
using Core.Models.Tutorials.Repositories;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class EnrolStudent
: IDomainEventHandler<TutorialRequestScheduledDomainEvent>
{
    private readonly ITutorialRepository _tutorialRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public EnrolStudent(
        ITutorialRepository tutorialRepository,
        IStudentRepository studentRepository,
        IEnrolmentRepository enrolmentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _tutorialRepository = tutorialRepository;
        _studentRepository = studentRepository;
        _enrolmentRepository = enrolmentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<TutorialRequestScheduledDomainEvent>();
    }

    public async Task Handle(TutorialRequestScheduledDomainEvent notification, CancellationToken cancellationToken)
    {
        Request tutorialRequest = await _tutorialRepository.GetRequestById(notification.RequestId, cancellationToken);

        if (tutorialRequest is null)
        {
            _logger
                .ForContext(nameof(TutorialRequestScheduledDomainEvent), notification, true)
                .ForContext(nameof(Error), TutorialRequestErrors.NotFound(notification.RequestId), true)
                .Warning("Failed to enrol student in new Tutorial");

            return;
        }

        if (tutorialRequest.Plan is null)
        {
            _logger
                .ForContext(nameof(TutorialRequestScheduledDomainEvent), notification, true)
                .ForContext(nameof(Error), TutorialRequestErrors.PlanNotFound(notification.RequestId), true)
                .Warning("Failed to enrol student in new Tutorial");

            return;
        }

        Student student = await _studentRepository.GetById(tutorialRequest.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(TutorialRequestScheduledDomainEvent), notification, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(tutorialRequest.StudentId), true)
                .Warning("Failed to enrol student in new Tutorial");

            return;
        }

        // Enrol student in new tutorial
        if (tutorialRequest.Plan.TutorialId == TutorialId.Empty)
        {
            _logger
                .ForContext(nameof(TutorialRequestScheduledDomainEvent), notification, true)
                .ForContext(nameof(Error), TutorialRequestErrors.PlanNotFound(notification.RequestId), true)
                .Warning("Failed to enrol student in new Tutorial");

            return;
        }

        Enrolment enrolment = TutorialEnrolment.Create(student.Id, tutorialRequest.Plan.TutorialId);

        _enrolmentRepository.Insert(enrolment);

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
