using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Presentation.Server.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Constellation.Presentation.Server.Areas.Subject.Models
{
    public class Course_DetailsViewModel : BaseViewModel
    {
        public CourseDto Course { get; set; }
        public ICollection<OfferingDto> Courses { get; set; }

        public class CourseDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Grade Grade { get; set; }
            public Faculty Faculty { get; set; }
            public string HeadTeacherName { get; set; }
            public decimal FTEValue { get; set; }
            public decimal FTECalculation { get; set; }

            public static CourseDto ConvertFromCourse(Course course)
            {
                var viewModel = new CourseDto
                {
                    Id = course.Id,
                    Name = course.Name,
                    Grade = course.Grade,
                    Faculty = course.Faculty,
                    HeadTeacherName = course.HeadTeacher.DisplayName,
                    FTEValue = course.FullTimeEquivalentValue
                };

                return viewModel;
            }
        }

        public class OfferingDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public ICollection<string> Teachers { get; set; }
            public bool IsCurrent { get; set; }
            public bool IsFuture { get; set; }
            public DateTime EndDate { get; set; }

            public static OfferingDto ConvertFromOffering(CourseOffering offering)
            {
                var viewModel = new OfferingDto
                {
                    Id = offering.Id,
                    Name = offering.Name,
                    Teachers = offering.Sessions.Where(session => !session.IsDeleted).Select(session => session.Teacher.DisplayName).Distinct().ToList(),
                    IsCurrent = offering.StartDate <= DateTime.Now && offering.EndDate >= DateTime.Now,
                    IsFuture = offering.StartDate > DateTime.Now,
                    EndDate = offering.EndDate
                };

                return viewModel;
            }
        }
    }
}