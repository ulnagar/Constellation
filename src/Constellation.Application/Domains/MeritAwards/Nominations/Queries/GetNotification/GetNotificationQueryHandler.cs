namespace Constellation.Application.Domains.MeritAwards.Nominations.Queries.GetNotification;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Abstractions.Services;
using Core.Models.Awards;
using Core.Models.Awards.Errors;
using Core.Shared;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetNotificationQueryHandler
: IQueryHandler<GetNotificationQuery, NotificationResponse>
{
    private readonly IAwardNominationRepository _awardRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public GetNotificationQueryHandler(
        IAwardNominationRepository awardRepository,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _awardRepository = awardRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<NotificationResponse>> Handle(GetNotificationQuery request, CancellationToken cancellationToken)
    {
        NominationNotification notification = await _awardRepository.GetNotificationById(request.NotificationId, cancellationToken);

        if (notification is null)
        {
            _logger
                .ForContext(nameof(GetNotificationQuery), request, true)
                .ForContext(nameof(Error), AwardNominationNotificationErrors.NotFound(request.NotificationId), true)
                .Warning("Failed to retrieve Award Nomination Notification by user {User}", _currentUserService.UserName);

            return Result.Failure<NotificationResponse>(AwardNominationNotificationErrors.NotFound(request.NotificationId));
        }

        return new NotificationResponse(
            notification.Type,
            notification.SentAt,
            notification.FromAddress,
            notification.ToAddresses.ToList(),
            notification.CcAddresses.ToList(),
            notification.Subject,
            notification.Body);
    }
}
