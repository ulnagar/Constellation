namespace Constellation.Application.MissedWork.GetNotificationDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.MissedWork;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetNotificationDetailsQueryHandler
    : IQueryHandler<GetNotificationDetailsQuery, NotificationDetails>
{
    private readonly IClassworkNotificationRepository _notificationRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ICourseOfferingRepository _offeringRepository;
    private readonly ILogger _logger;

    public GetNotificationDetailsQueryHandler(
        IClassworkNotificationRepository notificationRepository,
        IStudentRepository studentRepository,
        ICourseOfferingRepository offeringRepository,
        ILogger logger)
    {
        _notificationRepository = notificationRepository;
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
        _logger = logger.ForContext<GetNotificationDetailsQuery>();
    }
    public async Task<Result<NotificationDetails>> Handle(GetNotificationDetailsQuery request, CancellationToken cancellationToken)
    {
        ClassworkNotification notification = await _notificationRepository.GetById(request.NotificationId, cancellationToken);

        if (notification is null)
        {
            _logger.Warning("Could not find a classwork notification record with the Id {id}", request.NotificationId);

            return Result.Failure<NotificationDetails>(DomainErrors.MissedWork.ClassworkNotification.NotFound(request.NotificationId));
        }

        List<Student> students = await _studentRepository.GetListFromIds(notification.Absences.Select(absence => absence.StudentId).ToList(), cancellationToken);

        List<Name> studentNames = new();

        foreach (Student student in students)
        {
            studentNames.Add(student.GetName());
        }

        CourseOffering offering = await _offeringRepository.GetById(notification.OfferingId, cancellationToken);

        return new NotificationDetails(
            notification.Id,
            offering.Name,
            notification.AbsenceDate,
            studentNames,
            notification.Description);
    }
}
