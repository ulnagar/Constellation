using Constellation.Application.Features.Subject.Assignments.Models;
using MediatR;
using System;

namespace Constellation.Application.Features.Subject.Assignments.Queries
{
    public class GetAssignmentQuery : IRequest<AssignmentForList>
    {
        public Guid Id { get; set; }
    }
}
