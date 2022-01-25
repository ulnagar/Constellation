using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Services
{
    // Reviewed for ASYNC operations
    public class CourseOfferingService : ICourseOfferingService, IScopedService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CourseOfferingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceOperationResult<Course>> CreateCourse(CourseDto courseResource)
        {
            // Set up return object
            var result = new ServiceOperationResult<Course>();

            if (!await _unitOfWork.Courses.AnyWithId(courseResource.Id))
            {
                var course = new Course
                {
                    Name = courseResource.Name,
                    Grade = courseResource.Grade,
                    Faculty = courseResource.Faculty,
                    HeadTeacher = courseResource.HeadTeacher,
                    FullTimeEquivalentValue = courseResource.FullTimeEquivalentValue
                };

                _unitOfWork.Add(course);

                result.Success = true;
                result.Entity = course;
            }
            else
            {
                result.Success = false;
                result.Errors.Add($"A course with that Id already exists!");
            }

            return result;
        }

        public async Task<ServiceOperationResult<Course>> UpdateCourse(CourseDto courseResource)
        {
            // Set up return object
            var result = new ServiceOperationResult<Course>();

            // Validate entries
            var course = await _unitOfWork.Courses.ForEditAsync(courseResource.Id);

            if (course != null)
            {
                if (!string.IsNullOrWhiteSpace(courseResource.Name))
                    course.Name = courseResource.Name;

                if (courseResource.Grade != 0)
                    course.Grade = courseResource.Grade;

                if (courseResource.Faculty != 0)
                    course.Faculty = courseResource.Faculty;

                if (courseResource.FullTimeEquivalentValue != 0)
                    course.FullTimeEquivalentValue = courseResource.FullTimeEquivalentValue;

                course.HeadTeacherId = courseResource.HeadTeacherId;

                result.Success = true;
                result.Entity = course;
            }
            else
            {
                result.Success = false;
                result.Errors.Add($"A course with that Id already exists!");
            }

            return result;
        }

        public async Task RemoveCourse(int id)
        {
            // Validate entries
            var course = await _unitOfWork.Courses.ForEditAsync(id);

            if (course == null)
                return;

            _unitOfWork.Remove(course);
        }

        public async Task<ServiceOperationResult<CourseOffering>> CreateOffering(CourseOfferingDto offeringResource)
        {
            // Set up return object
            var result = new ServiceOperationResult<CourseOffering>();

            if (!await _unitOfWork.CourseOfferings.AnyWithId(offeringResource.Id))
            {
                var offering = new CourseOffering
                {
                    Name = offeringResource.Name,
                    CourseId = offeringResource.CourseId,
                    StartDate = offeringResource.StartDate,
                    EndDate = offeringResource.EndDate
                };

                _unitOfWork.Add(offering);

                result.Success = true;
                result.Entity = offering;
            }
            else
            {
                result.Success = false;
                result.Errors.Add($"An offering with that Id already exists!");
            }

            return result;
        }

        public async Task<ServiceOperationResult<CourseOffering>> UpdateOffering(CourseOfferingDto offeringResource)
        {
            // Set up return object
            var result = new ServiceOperationResult<CourseOffering>();

            // Validate entries
            var offering = await _unitOfWork.CourseOfferings.ForEditAsync(offeringResource.Id);

            if (offering != null)
            {
                if (!string.IsNullOrWhiteSpace(offeringResource.Name))
                    offering.Name = offeringResource.Name;

                if (offeringResource.CourseId != 0)
                    offering.CourseId = offeringResource.CourseId;

                offering.StartDate = offeringResource.StartDate;
                offering.EndDate = offeringResource.EndDate;

                result.Success = true;
                result.Entity = offering;
            }
            else
            {
                result.Success = false;
                result.Errors.Add($"An offering with that Id cannot be found!");
            }

            return result;
        }

        public async Task RemoveOffering(int id)
        {
            // Validate entries
            var offering = await  _unitOfWork.CourseOfferings.ForEditAsync(id);

            if (offering == null)
                return;

            _unitOfWork.Remove(offering);
        }
    }
}
