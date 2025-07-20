namespace Constellation.Application.Domains.Students.Events.StudentWithdrawnDomainEvent;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Core.Models.Enrolments.Repositories;
using Core.Models.Students.Events;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveClassEnrolments 
    : IDomainEventHandler<StudentWithdrawnDomainEvent>
{
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly ITutorialRepository _tutorialRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveClassEnrolments(
        IEnrolmentRepository enrolmentRepository,
        ITutorialRepository tutorialRepository,
        IOfferingRepository offeringRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _enrolmentRepository = enrolmentRepository;
        _tutorialRepository = tutorialRepository;
        _offeringRepository = offeringRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<StudentWithdrawnDomainEvent>();
    }

    public async Task Handle(StudentWithdrawnDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.Information("Attempting to unenroll student {studentId} from classes due to withdrawal", notification.StudentId);

        List<Enrolment> enrolments = await _enrolmentRepository.GetCurrentByStudentId(notification.StudentId, cancellationToken);

        foreach (Enrolment enrolment in enrolments)
        {
            switch (enrolment)
            {
                case OfferingEnrolment offeringEnrolment:
                    {
                        Offering offering = await _offeringRepository.GetById(offeringEnrolment.OfferingId, cancellationToken);

                        enrolment.Cancel();

                        _logger.Information("Student {studentId} removed from class {class}", notification.StudentId, offering.Name);

                        break;
                    }

                case TutorialEnrolment tutorialEnrolment:
                    {
                        Tutorial tutorial = await _tutorialRepository.GetById(tutorialEnrolment.TutorialId, cancellationToken);

                        enrolment.Cancel();

                        _logger.Information("Student {studentId} removed from tutorial {tutorial}", notification.StudentId, tutorial.Name);

                        break;
                    }
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
