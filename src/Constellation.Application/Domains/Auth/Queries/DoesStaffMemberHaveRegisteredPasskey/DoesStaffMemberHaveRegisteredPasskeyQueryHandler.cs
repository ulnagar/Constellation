namespace Constellation.Application.Domains.Auth.Queries.DoesStaffMemberHaveRegisteredPasskey;

using Application.Models.Identity.Repositories;
using Constellation.Application.Abstractions.Messaging;
using Core.Models.Auth;
using Core.Shared;

internal sealed class DoesStaffMemberHaveRegisteredPasskeyQueryHandler
    : IQueryHandler<DoesStaffMemberHaveRegisteredPasskeyQuery, bool>
{
    private readonly IIdentityRepository _identityRepository;


    public DoesStaffMemberHaveRegisteredPasskeyQueryHandler(
        IIdentityRepository identityRepository)
    {
        _identityRepository = identityRepository;
    }

    public async Task<Result<bool>> Handle(DoesStaffMemberHaveRegisteredPasskeyQuery request, CancellationToken cancellationToken)
    {
        AppUser? user = await _identityRepository.GetUserByEmail(request.EmailAddress, cancellationToken);

        return user.PasskeyCredentials.Any();
    }
}