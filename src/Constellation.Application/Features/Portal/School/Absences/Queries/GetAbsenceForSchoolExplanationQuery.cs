using Constellation.Application.Features.Portal.School.Absences.Models;
using MediatR;
using System;

namespace Constellation.Application.Features.Portal.School.Absences.Queries
{
    public class GetAbsenceForSchoolExplanationQuery : IRequest<WholeAbsenceForSchoolExplanation>
    {
        public Guid Id { get; set; }
    }
}
