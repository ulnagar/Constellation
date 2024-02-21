namespace Constellation.Application.SchoolContacts.GetContactRolesForSelectionList;

using Abstractions.Messaging;
using Core.Shared;
using Core.Models.SchoolContacts.Repositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetContactRolesForSelectionListQueryHandler
    : IQueryHandler<GetContactRolesForSelectionListQuery, List<string>>
{
    private readonly ISchoolContactRepository _contactRepository;

    public GetContactRolesForSelectionListQueryHandler(
        ISchoolContactRepository contactRepository)
    {
        _contactRepository = contactRepository;
    }

    public async Task<Result<List<string>>> Handle(GetContactRolesForSelectionListQuery request, CancellationToken cancellationToken) => 
        await _contactRepository.GetAvailableRoleList(cancellationToken);
}
