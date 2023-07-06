namespace Constellation.Application.MissedWork.CreateClassworkNotificationResponse;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.MissedWork;
using Constellation.Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateClassworkNotificationResponseCommandHandler
    : ICommandHandler<CreateClassworkNotificationResponseCommand>
{
    private readonly IClassworkNotificationRepository _notificationRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IUnitOfWork _unitOfWork;

    private readonly ILogger _logger;

    public CreateClassworkNotificationResponseCommandHandler(
        IClassworkNotificationRepository notificationRepository,
        IStaffRepository staffRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _notificationRepository = notificationRepository;
        _staffRepository = staffRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<CreateClassworkNotificationResponseCommand>();
    }

    public async Task<Result> Handle(CreateClassworkNotificationResponseCommand request, CancellationToken cancellationToken)
    {
        ClassworkNotification notification = await _notificationRepository.GetById(request.NotificationId, cancellationToken);

        if (notification is null)
        {
            _logger.Warning("Could not find a notification with Id {id}", request.NotificationId);

            return Result.Failure(DomainErrors.MissedWork.ClassworkNotification.NotFound(request.NotificationId));
        }

        Staff teacher = await _staffRepository.GetByEmailAddress(request.TeacherEmailAddress, cancellationToken);

        if (teacher is null)
        {
            _logger.Warning("Could not identify staff member from email address {email}", request.TeacherEmailAddress);

            return Result.Failure(DomainErrors.Partners.Staff.NotFoundByEmail(request.TeacherEmailAddress));
        }

        notification.RecordResponse(request.Description, teacher.DisplayName, teacher.StaffId);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
