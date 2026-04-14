namespace Constellation.Application.Domains.Auth.Queries.GetFilteredUsers;

using Abstractions.Messaging;
using Core.Models.Auth;
using Core.Shared;
using Models.Identity.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

internal sealed class GetFilteredUsersQueryHandler
: IQueryHandler<GetFilteredUsersQuery, List<AppUser>>
{
    private readonly IIdentityRepository _identityRepository;
    
    public GetFilteredUsersQueryHandler(
        IIdentityRepository identityRepository)
    {
        _identityRepository = identityRepository;
    }

    public async Task<Result<List<AppUser>>> Handle(GetFilteredUsersQuery request, CancellationToken cancellationToken)
    {
        return await _identityRepository.GetFilteredUsers(request.Filter, cancellationToken);
    }
}
