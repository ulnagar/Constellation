namespace Constellation.Application.Domains.EnrolmentContext.EnrolmentUser.Events.EnrolmentUserCreated;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.EnrolmentParent;
using Core.Models.EnrolmentContext.EnrolmentParent.Errors;
using Core.Models.EnrolmentContext.EnrolmentParent.Repositories;
using Core.Models.EnrolmentContext.EnrolmentUser;
using Core.Models.EnrolmentContext.EnrolmentUser.Errors;
using Core.Models.EnrolmentContext.EnrolmentUser.Events;
using Core.Models.EnrolmentContext.EnrolmentUser.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Interfaces;
using Serilog;

internal sealed class LinkToExistingApplications
    : IDomainEventHandler<EnrolmentUserEmailConfirmedDomainEvent>
{
    private readonly IEnrolmentUserRepository _userRepository;
    private readonly IEnrolmentParentRepository _parentRepository;
    private readonly IEnrolmentUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public LinkToExistingApplications(
        IEnrolmentUserRepository userRepository,
        IEnrolmentParentRepository parentRepository,
        IEnrolmentUnitOfWork unitOfWork,
        ILogger logger)
    {
        _userRepository = userRepository;
        _parentRepository = parentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<EnrolmentUserEmailConfirmedDomainEvent>();
    }

    public async Task Handle(EnrolmentUserEmailConfirmedDomainEvent notification, CancellationToken cancellationToken)
    {
        EnrolmentUser? user = await _userRepository.GetById(notification.UserId, cancellationToken);

        if (user is null)
        {
            _logger
                .ForContext(nameof(EnrolmentUserEmailConfirmedDomainEvent), notification, true)
                .ForContext(nameof(Error), EnrolmentUserErrors.NotFound(notification.UserId), true)
                .Warning("Failed to link user to existing applications.");

            return;
        }

        Result<EmailAddress> parentEmail = EmailAddress.Create(user.Email);

        if (parentEmail.IsFailure)
        {
            _logger
                .ForContext(nameof(EnrolmentUserEmailConfirmedDomainEvent), notification, true)
                .ForContext(nameof(Error), parentEmail.Error, true)
                .Warning("Failed to link user to existing applications.");
         
            return;
        }

        EnrolmentParent? parent = await _parentRepository.GetByEmail(parentEmail.Value, cancellationToken);

        if (parent is null)
        {
            _logger
                .ForContext(nameof(EnrolmentUserEmailConfirmedDomainEvent), notification, true)
                .ForContext(nameof(Error), EnrolmentParentErrors.NotFoundByEmail(parentEmail.Value), true)
                .Warning("Failed to link user to existing applications.");

            return;
        }

        if (parent.EnrolmentUserId.HasValue && parent.EnrolmentUserId.Value != user.Id)
        {
            _logger
                .ForContext(nameof(EnrolmentUserEmailConfirmedDomainEvent), notification, true)
                .ForContext(nameof(Error), EnrolmentParentErrors.InvalidId, true)
                .Warning("Failed to link user to existing applications.");

            return;
        }

        parent.UpdateUserLink(user.Id);

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
