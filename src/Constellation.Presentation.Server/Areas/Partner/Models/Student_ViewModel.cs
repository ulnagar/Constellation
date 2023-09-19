using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Presentation.Server.BaseModels;
using System.Collections.Generic;
using System.Linq;

namespace Constellation.Presentation.Server.Areas.Partner.Models
{
    public class Student_ViewModel : BaseViewModel
    {
        public ICollection<StudentDto> Students { get; set; }

        public class StudentDto
        {
            public StudentDto()
            {
                CurrentEnrolments = new List<string>();
            }

            public string StudentId { get; set; }
            public string Name { get; set; }
            public string Gender { get; set; }
            public Grade Grade { get; set; }
            public string SchoolName { get; set; }
            public List<string> CurrentEnrolments { get; set; }

            public static StudentDto ConvertFromStudent(Student student)
            {
                var viewModel = new StudentDto
                {
                    StudentId = student.StudentId,
                    Name = student.DisplayName,
                    Gender = student.Gender,
                    Grade = student.CurrentGrade,
                    SchoolName = student.School.Name
                };

                return viewModel;
            }
        }
    }
}