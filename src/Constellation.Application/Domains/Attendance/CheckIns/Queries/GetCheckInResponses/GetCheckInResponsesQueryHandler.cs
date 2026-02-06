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
        List<CheckInResponse> responses = await _checkInRepository.GetAll(cancellationToken);

        if (request.Filter is null)
            return responses;

        if (request.Filter.Grades.Count > 0)
        {
            responses = responses
                .Where(response => request.Filter.Grades.Contains(response.Grade))
                .ToList();
        }

        if (request.Filter.Courses.Count > 0)
        {
            responses = responses
                .Where(response => request.Filter.Courses.Select(CourseId.FromValue).Contains(response.CourseId))
                .ToList();
        }

        if (request.Filter.Offerings.Count > 0)
        {
            responses = responses
                .Where(response => request.Filter.Offerings.Select(OfferingId.FromValue).Contains(response.OfferingId))
                .ToList();
        }

        if (request.Filter.Schools.Count > 0)
        {
            responses = responses
                .Where(response => request.Filter.Schools.Contains(response.SchoolCode))
                .ToList();
        }

        if (request.Filter.Sentiments.Count > 0)
        {
            responses = responses
                .Where(response => request.Filter.Sentiments.Contains(response.Sentiment))
                .ToList();
        }

        return responses;
    }
}
