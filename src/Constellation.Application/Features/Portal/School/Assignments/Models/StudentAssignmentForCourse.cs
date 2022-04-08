using Constellation.Application.Common.Mapping;
using Constellation.Core.Models;
using System;

namespace Constellation.Application.Features.Portal.School.Assignments.Models
{
    public class StudentAssignmentForCourse : IMapFrom<CanvasAssignment>
    {
        public Guid Id { get; set; }
        public int CanvasId { get; set; }
        public string Name { get; set; }
        public DateTime DueDate { get; set; }
    }

}
