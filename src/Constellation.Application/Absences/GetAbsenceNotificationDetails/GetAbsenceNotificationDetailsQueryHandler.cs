namespace Constellation.Application.Absences.GetAbsenceNotificationDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.Absences;
using Constellation.Core.Shared;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAbsenceNotificationDetailsQueryHandler
    : IQueryHandler<GetAbsenceNotificationDetailsQuery, string>
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly ILogger _logger;

    public GetAbsenceNotificationDetailsQueryHandler(
        IAbsenceRepository absenceRepository,
        ILogger logger)
    {
        _absenceRepository = absenceRepository;
        _logger = logger.ForContext<GetAbsenceNotificationDetailsQuery>();
    }

    public async Task<Result<string>> Handle(GetAbsenceNotificationDetailsQuery request, CancellationToken cancellationToken)
    {
        Absence absence = await _absenceRepository.GetById(request.AbsenceId);

        if (absence is null)
        {
            _logger.Warning("Could not find Absence with Id {id}", request.AbsenceId);

            return Result.Failure<string>(DomainErrors.Absences.Absence.NotFound(request.AbsenceId));
        }

        Notification notification = absence.Notifications.FirstOrDefault(entry => entry.Id == request.NotificationId);

        if (notification is null)
        {
            _logger.Warning("Could not find Absence Notification with Id {id}", request.NotificationId);

            return Result.Failure<string>(DomainErrors.Absences.Notification.NotFound(request.NotificationId));
        }

        return notification.Message;
    }
}
