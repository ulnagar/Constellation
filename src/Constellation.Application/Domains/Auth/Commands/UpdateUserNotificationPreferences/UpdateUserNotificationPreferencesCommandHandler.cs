namespace Constellation.Application.Domains.Auth.Commands.UpdateUserNotificationPreferences;

using Abstractions.Messaging;
using Application.Models.Identity.Repositories;
using Core.Models.Auth;
using Core.Models.Auth.Enums;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;

internal sealed class UpdateUserNotificationPreferencesCommandHandler
: ICommandHandler<UpdateUserNotificationPreferencesCommand>
{
    private readonly IIdentityRepository _identityRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateUserNotificationPreferencesCommandHandler(
        IIdentityRepository identityRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _identityRepository = identityRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<UpdateUserNotificationPreferencesCommand>();
    }

    public async Task<Result> Handle(UpdateUserNotificationPreferencesCommand request, CancellationToken cancellationToken)
    {
        List<NotificationType> existingPreferences = await _identityRepository.GetOptedInNotificationTypesForUser(request.UserId, cancellationToken);

        foreach (var entry in request.Types)
        {
            if (existingPreferences.Contains(entry))
                continue;

            AppUserNotificationPreference preference = new()
            {
                AppUserId = request.UserId,
                NotificationType = entry
            };

            _identityRepository.Insert(preference);
        }

        foreach (var entry in existingPreferences)
        {
            if (request.Types.Contains(entry))
                continue;

            AppUserNotificationPreference preference = new()
            {
                AppUserId = request.UserId,
                NotificationType = entry
            };

            _identityRepository.Remove(preference);
        }
        
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
