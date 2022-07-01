using Constellation.Application.Features.Portal.School.Home.Models;
using MediatR;
using System.Collections.Generic;

namespace Constellation.Application.Features.Equipment.Stocktake.Queries
{
    public class GetStaffForSelectionQuery : IRequest<ICollection<StaffFromSchoolForDropdownSelection>>
    {
    }
}
