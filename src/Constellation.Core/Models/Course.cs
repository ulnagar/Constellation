using Constellation.Core.Enums;
using System.Collections.Generic;

namespace Constellation.Core.Models
{
    public class Course
    {
        public Course()
        {
            Offerings = new List<CourseOffering>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public Grade Grade { get; set; }
        public Faculty Faculty { get; set; }
        public Staff HeadTeacher { get; set; }
        public string HeadTeacherId { get; set; }
        public decimal FullTimeEquivalentValue { get; set; }
        public ICollection<CourseOffering> Offerings { get; set; }
    }
}