namespace Constellation.Infrastructure.Services;

using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Shared;
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
            Result<Course> request = Course.Create(
                courseResource.Name,
                courseResource.Code,
                courseResource.Grade,
                courseResource.FacultyId,
                courseResource.FullTimeEquivalentValue);

            if (request.IsSuccess)
            {
                _courseRepository.Insert(request.Value);

                await _unitOfWork.CompleteAsync();
            }

            result.Success = true;
            result.Entity = request.Value;
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
            Result request = course.Update(
                courseResource.Name,
                courseResource.Code,
                courseResource.Grade,
                courseResource.FacultyId,
                courseResource.FullTimeEquivalentValue);

            if (request.IsFailure)
            {
                result.Success = false;
                result.Errors.Add(request.Error.Message);

                return result;
            }

            result.Success = true;
            result.Entity = course;

            await _unitOfWork.CompleteAsync();
        }
        else
        {
            result.Success = false;
            result.Errors.Add($"A course with that Id already exists!");
        }

        return result;
    }

    public async Task RemoveCourse(CourseId id)
    {
        // Validate entries
        var course = await _courseRepository.ForEditAsync(id);

        if (course == null)
            return;

        _unitOfWork.Remove(course);
    }
}
