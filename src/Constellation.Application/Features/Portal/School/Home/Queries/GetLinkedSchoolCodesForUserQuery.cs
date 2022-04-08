using MediatR;
using System.Collections.Generic;

namespace Constellation.Application.Features.Portal.School.Home.Queries
{
    public class GetLinkedSchoolCodesForUserQuery : IRequest<ICollection<string>>
    {
        public string UserEmail { get; set; }
    }
}
