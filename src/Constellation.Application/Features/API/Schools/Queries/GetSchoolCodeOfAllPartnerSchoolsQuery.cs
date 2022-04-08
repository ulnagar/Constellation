using MediatR;
using System.Collections.Generic;

namespace Constellation.Application.Features.API.Schools.Queries
{
    public class GetSchoolCodeOfAllPartnerSchoolsQuery : IRequest<ICollection<string>>
    {
    }
}
