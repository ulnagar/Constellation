namespace Constellation.Infrastructure.Services;

using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects;
using Constellation.Infrastructure.DependencyInjection;

public class CourseOfferingService : ICourseOfferingService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CourseOfferingService(
        ICourseRepository courseRepository,
        IOfferingRepository offeringRepository,
        IUnitOfWork unitOfWork)
    {
        _courseRepository = courseRepository;
        _offeringRepository = offeringRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceOperationResult<Course>> CreateCourse(CourseDto courseResource)
    {
        // Set up return object
        var result = new ServiceOperationResult<Course>();

        if (!await _courseRepository.AnyWithId(courseResource.Id))
        {
            var course = new Course
            {
                Name = courseResource.Name,
                Grade = courseResource.Grade,
                Faculty = courseResource.Faculty,
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
        var course = await _courseRepository.ForEditAsync(courseResource.Id);

        if (course != null)
        {
            if (!string.IsNullOrWhiteSpace(courseResource.Name))
                course.Name = courseResource.Name;

            if (courseResource.Grade != 0)
                course.Grade = courseResource.Grade;

            course.FacultyId = courseResource.FacultyId;

            if (courseResource.FullTimeEquivalentValue != 0)
                course.FullTimeEquivalentValue = courseResource.FullTimeEquivalentValue;

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
        var course = await _courseRepository.ForEditAsync(id);

        if (course == null)
            return;

        _unitOfWork.Remove(course);
    }
}
