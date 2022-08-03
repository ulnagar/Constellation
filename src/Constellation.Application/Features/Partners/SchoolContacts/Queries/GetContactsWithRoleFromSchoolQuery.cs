using Constellation.Application.Features.Partners.SchoolContacts.Models;
using MediatR;
using System.Collections.Generic;

namespace Constellation.Application.Features.Partners.SchoolContacts.Queries
{
    public class GetContactsWithRoleFromSchoolQuery : IRequest<ICollection<ContactWithRoleForList>>
    {
        public string Code { get; set; }
    }
}
