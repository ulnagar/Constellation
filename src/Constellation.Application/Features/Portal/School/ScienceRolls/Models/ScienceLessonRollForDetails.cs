using Constellation.Application.Common.Mapping;
using Constellation.Core.Models;
using System;
using System.Collections.Generic;

namespace Constellation.Application.Features.Portal.School.ScienceRolls.Models
{
    public class ScienceLessonRollForDetails : IMapFrom<LessonRoll>
    {
        public Guid Id { get; set; }
        public string LessonName { get; set; }
        public DateTime LessonDueDate { get; set; }
        public DateTime? LessonDate { get; set; }
        public string SchoolContactFirstName { get; set; }
        public string SchoolContactLastName { get; set; }
        public string SchoolContactName => $"{SchoolContactFirstName} {SchoolContactLastName}";
        public string Comment { get; set; }
        public ICollection<RollAttendance> Attendance { get; set; } = new List<RollAttendance>();
        
        public class RollAttendance : IMapFrom<LessonRoll.LessonRollStudentAttendance>
        {
            public string StudentFirstName { get; set; }
            public string StudentLastName { get; set; } 
            public string StudentName => $"{StudentFirstName} {StudentLastName}";
            public bool Present { get; set; }
        }
    }
}
