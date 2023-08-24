namespace Constellation.Infrastructure.Services;

using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Infrastructure.DependencyInjection;

public class CourseOfferingService : ICourseOfferingService, IScopedService
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

    public async Task<ServiceOperationResult<Offering>> CreateOffering(CourseOfferingDto offeringResource)
    {
        // Set up return object
        var result = new ServiceOperationResult<Offering>();

        var existing = await _offeringRepository.GetById(offeringResource.Id);

        if (existing is null)
        {
            var offering = new Offering
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

    public async Task<ServiceOperationResult<Offering>> UpdateOffering(CourseOfferingDto offeringResource)
    {
        // Set up return object
        var result = new ServiceOperationResult<Offering>();

        // Validate entries
        var offering = await _offeringRepository.GetById(offeringResource.Id);

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

    public async Task RemoveOffering(OfferingId id)
    {
        // Validate entries
        var offering = await  _offeringRepository.GetById(id);

        if (offering == null)
            return;

        _unitOfWork.Remove(offering);
    }
}
