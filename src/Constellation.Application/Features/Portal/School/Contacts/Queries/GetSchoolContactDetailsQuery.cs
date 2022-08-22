using Constellation.Application.Features.Portal.School.Contacts.Models;
using MediatR;

namespace Constellation.Application.Features.Portal.School.Contacts.Queries
{
    public class GetSchoolContactDetailsQuery : IRequest<SchoolContactDetails>
    {
        public string Code { get; set; }
    }
}
