namespace Constellation.Application.SchoolContacts.GetContactRolesForSelectionList;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Shared;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetContactRolesForSelectionListQueryHandler
    : IQueryHandler<GetContactRolesForSelectionListQuery, List<string>>
{
    private readonly ISchoolContactRoleRepository _roleRepository;

    public GetContactRolesForSelectionListQueryHandler(
        ISchoolContactRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Result<List<string>>> Handle(GetContactRolesForSelectionListQuery request, CancellationToken cancellationToken)
    {
        List<string> roles = (List<string>)await _roleRepository.ListOfRolesForSelectionAsync();

        return roles;
    }
}
