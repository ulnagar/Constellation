namespace Constellation.Application.Students.Events.StudentWithdrawnDomainEvent;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Core.Models.Students.Events;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveClassEnrolments 
    : IDomainEventHandler<StudentWithdrawnDomainEvent>
{
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveClassEnrolments(
        IEnrolmentRepository enrolmentRepository,
        IOfferingRepository offeringRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _enrolmentRepository = enrolmentRepository;
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
            Offering offering = await _offeringRepository.GetById(enrolment.OfferingId, cancellationToken);

            enrolment.Cancel();

            _logger.Information("Student {studentId} removed from class {class}", notification.StudentId, offering.Name);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
