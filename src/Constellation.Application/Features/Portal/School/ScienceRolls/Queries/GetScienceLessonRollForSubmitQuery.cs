using Constellation.Application.Features.Portal.School.ScienceRolls.Models;
using MediatR;
using System;

namespace Constellation.Application.Features.Portal.School.ScienceRolls.Queries
{
    public class GetScienceLessonRollForSubmitQuery : IRequest<ScienceLessonRollForSubmit>
    {
        public Guid RollId { get; set; }
    }
}
