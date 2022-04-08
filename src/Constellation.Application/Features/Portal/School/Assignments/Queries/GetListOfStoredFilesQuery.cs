using Constellation.Core.Models;
using MediatR;
using System.Collections.Generic;

namespace Constellation.Application.Features.Portal.School.Assignments.Queries
{
    public class GetListOfStoredFilesQuery : IRequest<ICollection<StoredFile>>
    {
    }
}
