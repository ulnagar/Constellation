using Constellation.Application.Features.Subject.Assignments.Models;
using MediatR;
using System;
using System.Collections.Generic;

namespace Constellation.Application.Features.Subject.Assignments.Queries
{
    public class GetAssignmentSubmissionsQuery : IRequest<ICollection<AssignmentSubmissionForList>>
    {
        public Guid Id { get; set; }
    }
}
