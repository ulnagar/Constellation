using Constellation.Core.Models;
using MediatR;

namespace Constellation.Application.Features.Auth.Queries
{
    public class GetSchoolContactByEmailAddressQuery : IRequest<SchoolContact>
    {
        public string EmailAddress { get; set; }
    }
}
