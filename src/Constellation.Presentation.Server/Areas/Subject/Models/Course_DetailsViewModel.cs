using Constellation.Core.Enums;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects;
using Constellation.Presentation.Server.BaseModels;

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
            public string Faculty { get; set; }
            public decimal FTEValue { get; set; }
            public decimal FTECalculation { get; set; }

            public static CourseDto ConvertFromCourse(Course course)
            {
                var viewModel = new CourseDto
                {
                    Id = course.Id,
                    Name = course.Name,
                    Grade = course.Grade,
                    Faculty = course.Faculty.Name,
                    FTEValue = course.FullTimeEquivalentValue
                };

                return viewModel;
            }
        }

        public class OfferingDto
        {
            public OfferingId Id { get; set; }
            public string Name { get; set; }
            public ICollection<string> Teachers { get; set; }
            public bool IsCurrent { get; set; }
            public bool IsFuture { get; set; }
            public DateTime EndDate { get; set; }

            public static OfferingDto ConvertFromOffering(Offering offering)
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