namespace Constellation.Infrastructure.Features.Partners.Students.Notifications.StudentWithdrawn;

using Constellation.Application.Features.Partners.Students.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.Offerings;
using MediatR;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

public class RemoveClassEnrolments : INotificationHandler<StudentWithdrawnNotification>
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
        _logger = logger.ForContext<StudentWithdrawnNotification>();
    }

    public async Task Handle(StudentWithdrawnNotification notification, CancellationToken cancellationToken)
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
