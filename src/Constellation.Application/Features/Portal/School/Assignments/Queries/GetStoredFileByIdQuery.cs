using Constellation.Core.Models;
using MediatR;

namespace Constellation.Application.Features.Portal.School.Assignments.Queries
{
    public class GetStoredFileByIdQuery : IRequest<StoredFile>
    {
        public int Id { get; set; }
    }
}
