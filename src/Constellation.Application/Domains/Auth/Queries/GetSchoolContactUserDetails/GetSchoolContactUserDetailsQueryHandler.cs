namespace Constellation.Application.Domains.Auth.Queries.GetSchoolContactUserDetails;

using Abstractions.Messaging;
using Application.Models.Identity.Errors;
using Application.Models.Identity.Repositories;
using Core.Models.Auth;
using Core.Models.Auth.Enums;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Identifiers;
using Core.Models.SchoolContacts.Repositories;
using Core.Shared;
using Serilog;

internal sealed class GetSchoolContactUserDetailsQueryHandler
: IQueryHandler<GetSchoolContactUserDetailsQuery, ContactUserResponse>
{
    private readonly IIdentityRepository _identityRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly ILogger _logger;

    public GetSchoolContactUserDetailsQueryHandler(
        IIdentityRepository identityRepository,
        ISchoolContactRepository contactRepository,
        ILogger logger)
    {
        _identityRepository = identityRepository;
        _contactRepository = contactRepository;
        _logger = logger
            .ForContext<GetSchoolContactUserDetailsQuery>();
    }

    public async Task<Result<ContactUserResponse>> Handle(GetSchoolContactUserDetailsQuery request, CancellationToken cancellationToken)
    {
        AppUser? user = await _identityRepository.GetUser(request.Id, cancellationToken);

        if (user is null)
        {
            _logger
                .ForContext(nameof(GetSchoolContactUserDetailsQuery), request, true)
                .ForContext(nameof(Error), AuthErrors.UserNotFound(request.Id), true)
                .Warning("Failed to retrieve User details");

            return Result.Failure<ContactUserResponse>(AuthErrors.UserNotFound(request.Id));
        }

        List<ContactUserResponse.Role> roles = [];

        foreach (var userLink in user.Links.Where(entry => !entry.IsDeleted && entry.Type == LinkType.Contact))
        {
            SchoolContactId contactId = SchoolContactId.FromValue(userLink.LinkId);

            if (contactId == SchoolContactId.Empty)
                continue;

            SchoolContact? contact = await _contactRepository.GetById(contactId, cancellationToken);

            if (contact is null)
                continue;

            foreach (var role in contact.Assignments.Where(entry => !entry.IsDeleted))
            {
                roles.Add(new(
                    role.Role,
                    role.SchoolName));
            }
        }
        
        List<ContactUserResponse.Passkey> passkeys = [];

        foreach (AppUserPasskey passkey in user.PasskeyCredentials)
        {
            passkeys.Add(new(
                passkey.Name,
                passkey.CreatedAt,
                passkey.CredentialId));
        }

        List<NotificationType> optedInNotifications = await _identityRepository.GetOptedInNotificationTypesForUser(user.Id, cancellationToken);

        ContactUserResponse response = new(
            user.Id,
            user.Name,
            user.Email,
            roles,
            passkeys,
            optedInNotifications);

        return response;
    }
}
