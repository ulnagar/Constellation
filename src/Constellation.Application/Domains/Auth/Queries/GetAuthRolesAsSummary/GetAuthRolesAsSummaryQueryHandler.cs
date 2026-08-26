namespace Constellation.Application.Domains.Auth.Queries.GetAuthRolesAsSummary;

using Abstractions.Messaging;
using Application.Models.Identity;
using Application.Models.Identity.Repositories;
using Core.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

internal sealed record GetAuthRolesAsSummaryQueryHandler
: IQueryHandler<GetAuthRolesAsSummaryQuery, List<RoleSummaryResponse>>
{
    private readonly IIdentityRepository _identityRepository;

    public GetAuthRolesAsSummaryQueryHandler(
        IIdentityRepository identityRepository)
    {
        _identityRepository = identityRepository;
    }

    public async Task<Result<List<RoleSummaryResponse>>> Handle(GetAuthRolesAsSummaryQuery request, CancellationToken cancellationToken)
    {
        List<RoleSummaryResponse> response = [];

        List<AppRole> roles = await _identityRepository.GetRoles(cancellationToken);

        foreach (AppRole role in roles)
        {
            int userCount = await _identityRepository.GetUserCountInRole(role.Name!, cancellationToken);

            response.Add(new(role.Id, role.Name, role.Type, userCount));
        }

        return response;
    }
}
