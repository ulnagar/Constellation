namespace Constellation.Application.Domains.SchoolContacts.Queries.GetContactRolesForSelectionList;

using Abstractions.Messaging;
using Core.Models.SchoolContacts.Enums;
using Core.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetContactRolesForSelectionListQueryHandler
    : IQueryHandler<GetContactRolesForSelectionListQuery, List<Position>>
{
    public async Task<Result<List<Position>>> Handle(GetContactRolesForSelectionListQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Position> options = Position.GetOptions
            .Where(position => position != Position.Empty);
        
        options = (request.IncludeRestrictedContacts) 
            ? options 
            : options.Where(position => !position.IsRestricted);

        return options
            .OrderBy(position => position.SortOrder)
            .ToList();
    }
}
