namespace Constellation.Application.Domains.EnrolmentContext.EnrolmentParent.Events.EnrolmentParentCreated;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.EnrolmentParent;
using Core.Models.EnrolmentContext.EnrolmentParent.Events;
using Core.Models.EnrolmentContext.EnrolmentParent.Repositories;
using Core.Models.EnrolmentContext.EnrolmentUser;
using Core.Models.EnrolmentContext.EnrolmentUser.Repositories;
using Serilog;

internal sealed class LinkEnrolmentsUser
: IDomainEventHandler<EnrolmentParentCreatedDomainEvent>
{
    private readonly IEnrolmentParentRepository _parentRepository;
    private readonly IEnrolmentUserRepository _userRepository;
    private readonly ILogger _logger;

    public LinkEnrolmentsUser(
        IEnrolmentParentRepository parentRepository,
        IEnrolmentUserRepository userRepository,
        ILogger logger)
    {
        _parentRepository = parentRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task Handle(EnrolmentParentCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        EnrolmentParent? parent = await _parentRepository.GetById(notification.ParentId, cancellationToken);

        if (parent is null)
        {
            _logger;

            return;
        }



        if (parent.EnrolmentUserId.HasValue)
        {
            var user = await _userRepository.GetById(parent.EnrolmentUserId.Value, cancellationToken);

            if (user is null)
            {
                user = CreateUser();

                parent.UpdateUserLink(user.Id);
            }

        }

    }
}
