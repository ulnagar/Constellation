using System.Collections.Generic;

namespace Constellation.Application.DTOs
{
    public class InterviewExportDto
    {
        public InterviewExportDto()
        {
            Parents = new List<Parent>();
        }

        public string StudentId { get; set; }
        public string StudentFirstName { get; set; }
        public string StudentLastName { get; set; }
        public ICollection<Parent> Parents { get; set; }
        public string ClassCode { get; set; }
        public string ClassGrade { get; set; }
        public string ClassName { get; set; }
        public string TeacherCode { get; set; }
        public string TeacherTitle { get; set; }
        public string TeacherFirstName { get; set; }
        public string TeacherLastName { get; set; }
        public string TeacherEmailAddress { get; set; }

        public class Parent
        {
            public string ParentCode { get; set; }
            public string ParentTitle { get; set; }
            public string ParentFirstName { get; set; }
            public string ParentLastName { get; set; }
            public string ParentEmailAddress { get; set; }
        }
    }
}
