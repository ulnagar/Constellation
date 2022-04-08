using Constellation.Application.Features.Subject.Assignments.Models;
using MediatR;
using System.Collections.Generic;

namespace Constellation.Application.Features.Subject.Assignments.Queries
{
    public class GetAssignmentsQuery : IRequest<ICollection<AssignmentForList>>
    {
    }
}
