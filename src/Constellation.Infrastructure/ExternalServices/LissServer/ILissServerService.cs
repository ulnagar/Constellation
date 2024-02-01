namespace Constellation.Infrastructure.ExternalServices.LissServer;

using Application.Interfaces.Repositories;
using Application.Offerings.CreateOffering;
using Constellation.Infrastructure.ExternalServices.LissServer.Models;
using Core.Abstractions.Clock;
using Core.Enums;
using Core.Models.Offerings;
using Core.Models.Offerings.Identifiers;
using Core.Models.Offerings.Repositories;
using Core.Models.Subjects;
using Core.Shared;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

public interface ILissServerService
{
    Task<ILissResponse> PublishStudents(object[] request);
    Task<ILissResponse> PublishTimetable(object[] request);
    Task<ILissResponse> PublishTeachers(object[] request);
    Task<ILissResponse> PublishClassMemberships(object[] request);
    Task<ILissResponse> PublishClasses(object[] request);
}

internal sealed class LissServerService : ILissServerService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly ISender _mediator;

    public LissServerService(
        ICourseRepository courseRepository,
        IOfferingRepository offeringRepository,
        IDateTimeProvider dateTime,
        ISender mediator)
    {
        _courseRepository = courseRepository;
        _offeringRepository = offeringRepository;
        _dateTime = dateTime;
        _mediator = mediator;
    }

    public async Task<ILissResponse> PublishStudents(object[] request)
    {
        if (request.Length != 3)
        {
            return LissResponseError.InvalidParameters;
        }

        List<LissPublishStudents> students = JsonSerializer.Deserialize<List<LissPublishStudents>>(request[2].ToString());

        // Do something with the new student objects

        return new LissResponseBlank();
    }

    public async Task<ILissResponse> PublishTimetable(object[] request)
    {
        if (request.Length != 8)
        {
            return LissResponseError.InvalidParameters;
        }

        List<LissPublishTimetable> timetables = JsonSerializer.Deserialize<List<LissPublishTimetable>>(request[1].ToString());

        // Do something with the new timetable objects

        return new LissResponseBlank();
    }

    public async Task<ILissResponse> PublishTeachers(object[] request)
    {
        if (request.Length != 3)
        {
            return LissResponseError.InvalidParameters;
        }

        List<LissPublishTeachers> teachers = JsonSerializer.Deserialize<List<LissPublishTeachers>>(request[2].ToString());

        // Do something with the new teacher objects

        return new LissResponseBlank();
    }

    public async Task<ILissResponse> PublishClassMemberships(object[] request)
    {
        if (request.Length != 3)
        {
            return LissResponseError.InvalidParameters;
        }

        var data = request[2].ToString();

        List<LissPublishClassMemberships> classMemberships = JsonSerializer.Deserialize<List<LissPublishClassMemberships>>(request[1].ToString());

        // Do something with the new class membership objects

        return new LissResponseBlank();
    }

    public async Task<ILissResponse> PublishClasses(object[] request)
    {
        if (request.Length != 3)
        {
            return LissResponseError.InvalidParameters;
        }

        List<LissPublishClasses> classes = JsonSerializer.Deserialize<List<LissPublishClasses>>(request[2].ToString());
        int year = JsonSerializer.Deserialize<int>(request[1].ToString());
        
        List<Course> courses = await _courseRepository.GetAll();

        List<string> classNames = classes
            .Select(entry => 
                entry.ClassCode
                    .Replace(" ", string.Empty)
                    .PadLeft(7, '0'))
            .ToList();

        // Do something with the new class objects
        foreach (LissPublishClasses entry in classes)
        {
            string className = entry.ClassCode.Replace(" ", string.Empty).PadLeft(7, '0');

            if (className.Length > 7)
            {
                // Invalid class name
                continue;
            }

            string courseCode = entry.ClassCode.Split(' ')[0];

            bool parseSuccess = int.TryParse(courseCode.Substring(0, courseCode.Length - 3), out int lissGrade);
            if (!parseSuccess)
            {
                // Invalid grade
                continue;
            }
            Grade grade = (Grade)lissGrade;
            
            string lissCourseCode = courseCode.Substring(courseCode.Length - 3, 3);
            
            Course course = courses.FirstOrDefault(record => record.Grade == (Grade)lissGrade && record.Code == lissCourseCode);

            if (course is null)
            {
                // Could not identify course
                continue;
            }

            List<Offering> offerings = await _offeringRepository.GetByCourseId(course.Id);

            Offering currentOffering = offerings.FirstOrDefault(offering => 
                offering.EndDate.Year == year && 
                offering.Name == className);

            if (currentOffering is null)
            {
                DateOnly startDate = _dateTime.FirstDayOfYear;
                DateOnly endDate = _dateTime.LastDayOfYear;

                // Calculate date ranges
                if (grade is Grade.Y11 or Grade.Y12)
                {
                    endDate = new DateOnly(year, 9, 30);
                }

                if (grade == Grade.Y12)
                {
                    startDate = new DateOnly(year - 1, 10, 1);
                }

                // Create a new Offering
                CreateOfferingCommand command = new(
                    className,
                    course.Id,
                    startDate,
                    endDate);

                //Result<OfferingId> result = await _mediator.Send(command);

                //if (result.IsFailure)
                //{
                //    // Log something?
                //}
            }
            else
            {
                // Do nothing?
            }
        }

        return new LissResponseBlank();
    }
}