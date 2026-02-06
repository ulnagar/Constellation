namespace Constellation.Application.Domains.Attendance.CheckIns.Queries.GetCheckInResponses;

using Abstractions.Messaging;
using Core.Models.Attendance.Checkin;
using Core.Models.Attendance.Repositories;
using Core.Models.Offerings.Identifiers;
using Core.Models.Subjects.Identifiers;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;

internal sealed class GetCheckInResponsesQueryHandler
: IQueryHandler<GetCheckInResponsesQuery, List<CheckInResponse>>
{
    private readonly ICheckInRepository _checkInRepository;
    private readonly ILogger _logger;

    public GetCheckInResponsesQueryHandler(
        ICheckInRepository checkInRepository,
        ILogger logger)
    {
        _checkInRepository = checkInRepository;
        _logger = logger;
    }

    public async Task<Result<List<CheckInResponse>>> Handle(GetCheckInResponsesQuery request, CancellationToken cancellationToken)
    {
        if (request.Filter is null)
            return await _checkInRepository.GetAll(cancellationToken);

        List<CheckInResponse> responses = [];

        if (request.Filter.Grades.Count > 0 && request.Filter.Schools.Count > 0)
        {
            foreach (var grade in request.Filter.Grades)
            {
                List<CheckInResponse> gradeResponses = await _checkInRepository.GetFromGrade(grade, cancellationToken);

                foreach (CheckInResponse response in gradeResponses)
                {
                    if (request.Filter.Schools.Contains(response.SchoolCode) && 
                        !responses.Contains(response))
                        responses.Add(response);
                }
            }

            return responses;
        }

        if (request.Filter.Grades.Count > 0)
        {
            foreach (var grade in request.Filter.Grades)
            {
                List<CheckInResponse> gradeResponses = await _checkInRepository.GetFromGrade(grade, cancellationToken);

                foreach (CheckInResponse response in gradeResponses)
                {
                    if (!responses.Contains(response))
                        responses.Add(response);
                }
            }

            return responses;
        }

        if (request.Filter.Courses.Count > 0 && request.Filter.Schools.Count > 0)
        {
            foreach (Guid course in request.Filter.Courses)
            {
                CourseId courseId = CourseId.FromValue(course);

                List<CheckInResponse> courseResponses = await _checkInRepository.GetFromCourse(courseId, cancellationToken);

                foreach (CheckInResponse response in courseResponses)
                {
                    if (request.Filter.Schools.Contains(response.SchoolCode) &&
                        !responses.Contains(response))
                        responses.Add(response);
                }
            }

            return responses;
        }

        if (request.Filter.Courses.Count > 0)
        {
            foreach (Guid course in request.Filter.Courses)
            {
                CourseId courseId = CourseId.FromValue(course);

                List<CheckInResponse> courseResponses = await _checkInRepository.GetFromCourse(courseId, cancellationToken);

                foreach (CheckInResponse response in courseResponses)
                {
                    if (!responses.Contains(response))
                        responses.Add(response);
                }
            }

            return responses;
        }

        if (request.Filter.Offerings.Count > 0 && request.Filter.Schools.Count > 0)
        {
            foreach (Guid offering in request.Filter.Offerings)
            {
                OfferingId offeringId = OfferingId.FromValue(offering);

                List<CheckInResponse> offeringResponses = await _checkInRepository.GetFromOffering(offeringId, cancellationToken);

                foreach (CheckInResponse response in offeringResponses)
                {
                    if (request.Filter.Schools.Contains(response.SchoolCode) &&
                        !responses.Contains(response))
                        responses.Add(response);
                }
            }

            return responses;
        }

        if (request.Filter.Offerings.Count > 0)
        {
            foreach (Guid offering in request.Filter.Offerings)
            {
                OfferingId offeringId = OfferingId.FromValue(offering);

                List<CheckInResponse> offeringResponses = await _checkInRepository.GetFromOffering(offeringId, cancellationToken);

                foreach (CheckInResponse response in offeringResponses)
                {
                    if (!responses.Contains(response))
                        responses.Add(response);
                }
            }

            return responses;
        }

        if (request.Filter.Schools.Count > 0)
        {
            foreach (string schoolCode in request.Filter.Schools)
            {
                List<CheckInResponse> schoolResponses = await _checkInRepository.GetFromSchool(schoolCode, cancellationToken);

                foreach (CheckInResponse response in schoolResponses)
                {
                    if (!responses.Contains(response))
                        responses.Add(response);
                }
            }

            return responses;
        }

        return responses;
    }
}
