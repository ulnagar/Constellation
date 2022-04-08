using MediatR;

namespace Constellation.Application.Features.Portal.School.Home.Queries
{
    public class IsPartnerSchoolWithStudentsQuery : IRequest<bool>
    {
        public string SchoolCode { get; set; }
    }
}
