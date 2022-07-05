using Constellation.Application.Features.Awards.Models;
using MediatR;
using System.Collections.Generic;

namespace Constellation.Application.Features.Awards.Queries
{
    public class GetStudentsWithAwardQuery : IRequest<ICollection<StudentWithAwards>>
    {
    }
}
