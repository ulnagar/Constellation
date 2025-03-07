namespace Constellation.Application.SchoolContacts.GetContactRolesForSelectionList;

using Abstractions.Messaging;
using Core.Models.SchoolContacts;
using Core.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetContactRolesForSelectionListQueryHandler
    : IQueryHandler<GetContactRolesForSelectionListQuery, List<string>>
{
    public async Task<Result<List<string>>> Handle(GetContactRolesForSelectionListQuery request, CancellationToken cancellationToken)
    {
        List<string> contactTypes = new();

        if (request.IncludeRestrictedContacts)
            contactTypes.Add(SchoolContactRole.Principal);

        contactTypes.Add(SchoolContactRole.Coordinator);
        contactTypes.Add(SchoolContactRole.SciencePrac);

        return contactTypes.OrderBy(entry => entry).ToList();
    }
}
