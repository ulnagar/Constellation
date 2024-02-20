using Constellation.Core.Models.SchoolContacts;
using MediatR;

namespace Constellation.Application.Features.Auth.Queries
{
    public class GetSchoolContactByEmailAddressQuery : IRequest<SchoolContact>
    {
        public string EmailAddress { get; set; }
    }
}
