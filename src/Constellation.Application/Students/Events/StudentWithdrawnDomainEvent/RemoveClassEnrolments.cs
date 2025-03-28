namespace Constellation.Application.Students.Events.StudentWithdrawnDomainEvent;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Core.Models.OfferingEnrolments;
using Core.Models.OfferingEnrolments.Repositories;
using Core.Models.Students.Events;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveClassEnrolments 
    : IDomainEventHandler<StudentWithdrawnDomainEvent>
{
    private readonly IOfferingEnrolmentRepository _offeringEnrolmentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveClassEnrolments(
        IOfferingEnrolmentRepository offeringEnrolmentRepository,
        IOfferingRepository offeringRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offeringEnrolmentRepository = offeringEnrolmentRepository;
        _offeringRepository = offeringRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<StudentWithdrawnDomainEvent>();
    }

    public async Task Handle(StudentWithdrawnDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.Information("Attempting to unenroll student {studentId} from classes due to withdrawal", notification.StudentId);

        List<OfferingEnrolment> enrolments = await _offeringEnrolmentRepository.GetCurrentByStudentId(notification.StudentId, cancellationToken);

        foreach (OfferingEnrolment enrolment in enrolments)
        {
            Offering offering = await _offeringRepository.GetById(enrolment.OfferingId, cancellationToken);

            enrolment.Cancel();

            _logger.Information("Student {studentId} removed from class {class}", notification.StudentId, offering.Name);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
