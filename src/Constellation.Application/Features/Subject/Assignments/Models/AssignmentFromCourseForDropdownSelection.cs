using Constellation.Application.Common.Mapping;
using Constellation.Application.DTOs;
using System;

namespace Constellation.Application.Features.Subject.Assignments.Models
{
    public class AssignmentFromCourseForDropdownSelection : IMapFrom<CanvasAssignmentDto>
    {
        public string Name { get; set; }
        public int CanvasId { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? LockDate { get; set; }
        public DateTime? UnlockDate { get; set; }
        public int AllowedAttempts { get; set; }
        public bool ExistsInDatabase { get; set; }
    }
}
