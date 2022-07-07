using MediatR;
using System.Collections.Generic;

namespace Constellation.Application.Features.Portal.School.Home.Queries
{
    public class GetAllPartnerSchoolCodesQuery : IRequest<ICollection<string>>
    {
    }
}
