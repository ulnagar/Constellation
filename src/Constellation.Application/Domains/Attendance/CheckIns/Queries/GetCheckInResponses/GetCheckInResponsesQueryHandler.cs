namespace Constellation.Application.Domains.Attendance.CheckIns.Queries.GetCheckInResponses;

using Abstractions.Messaging;
using Core.Models.Attendance.Checkin;
using Core.Models.Attendance.Repositories;
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
        if (request.Grade is not null)
            return await _checkInRepository.GetFromGrade(request.Grade.Value, cancellationToken);

        if (request.CourseId is not null)
            return await _checkInRepository.GetFromCourse(request.CourseId.Value, cancellationToken);

        if (request.OfferingId is not null)
            return await _checkInRepository.GetFromOffering(request.OfferingId.Value, cancellationToken);

        if (request.SchoolCode is not null)
            return await _checkInRepository.GetFromSchool(request.SchoolCode, cancellationToken);

        return await _checkInRepository.GetAll(cancellationToken);
    }
}
