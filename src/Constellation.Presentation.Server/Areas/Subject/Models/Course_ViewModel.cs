using Constellation.Application.Features.Faculties.Models;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Subjects;
using Constellation.Presentation.Server.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Constellation.Presentation.Server.Areas.Subject.Models
{
    public class Course_ViewModel : BaseViewModel
    {
        public Course_ViewModel()
        {
            Courses = new List<CourseDto>();
        }

        public ICollection<CourseDto> Courses { get; set; }
        public IDictionary<Guid, string> FacultyList { get; set; } = new Dictionary<Guid, string>();

        public class CourseDto
        {
            public CourseDto()
            {
                Offerings = new List<OfferingDto>();
            }

            public int Id { get; set; }
            public string Name { get; set; }
            public Grade Grade { get; set; }
            public Faculty Faculty { get; set; }
            public ICollection<OfferingDto> Offerings { get; set; }

            public static CourseDto ConvertFromCourse(Course course)
            {
                var viewModel = new CourseDto
                {
                    Id = course.Id,
                    Name = course.Name,
                    Grade = course.Grade,
                    Faculty = course.Faculty,
                    Offerings = course.Offerings.Select(OfferingDto.ConvertFromOffering).ToList()
                };

                return viewModel;
            }
        }

        public class OfferingDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool IsCurrent { get; set; }
            public bool IsFuture { get; set; }

            public static OfferingDto ConvertFromOffering(Offering offering)
            {
                var viewModel = new OfferingDto
                {
                    Id = offering.Id,
                    Name = offering.Name,
                    IsCurrent = offering.StartDate <= DateTime.Now && offering.EndDate >= DateTime.Now,
                    IsFuture = offering.StartDate > DateTime.Now
                };

                return viewModel;
            }
        }
    }
}