using Constellation.Application.Features.Awards.Models;
using MediatR;

namespace Constellation.Application.Features.Awards.Queries
{
    public class GetStudentWithAwardQuery : IRequest<StudentWithAwards>
    {
        public string StudentId { get; set; }
    }
}
