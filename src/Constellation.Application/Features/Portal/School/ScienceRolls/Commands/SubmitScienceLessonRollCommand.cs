using MediatR;
using System;
using System.Collections.Generic;

namespace Constellation.Application.Features.Portal.School.ScienceRolls.Commands
{
    public class SubmitScienceLessonRollCommand : IRequest
    {
        public Guid RollId { get; set; }
        public DateTime LessonDate { get; set; }
        public string Comment { get; set; }
        public string UserEmail { get; set; }
        public IDictionary<Guid, bool> Attendance { get; set; } = new Dictionary<Guid, bool>();
    }
}
