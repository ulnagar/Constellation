using Constellation.Application.Features.Portal.School.Home.Models;
using MediatR;
using System.Collections.Generic;

namespace Constellation.Application.Features.Portal.School.Home.Queries
{
    public class GetStaffFromSchoolForSelectionQuery : IRequest<ICollection<StaffFromSchoolForDropdownSelection>>
    {
        public string SchoolCode { get; set; }
    }
}
