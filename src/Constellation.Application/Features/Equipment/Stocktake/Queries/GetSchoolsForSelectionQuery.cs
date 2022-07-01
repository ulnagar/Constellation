using Constellation.Application.Features.Equipment.Stocktake.Models;
using MediatR;
using System.Collections.Generic;

namespace Constellation.Application.Features.Equipment.Stocktake.Queries
{
    public class GetSchoolsForSelectionQuery : IRequest<ICollection<PartnerSchoolForDropdownSelection>>
    {
    }
}
