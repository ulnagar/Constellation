using Constellation.Application.Features.Portal.School.ScienceRolls.Models;
using MediatR;
using System;

namespace Constellation.Application.Features.Portal.School.ScienceRolls.Queries
{
    public class GetScienceLessonRollForDisplayQuery : IRequest<ScienceLessonRollForDetails>
    {
        public Guid RollId { get; set; }
    }
}
