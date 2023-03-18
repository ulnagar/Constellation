namespace Constellation.Application.Offerings.GetSessionDetailsForStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetSessionDetailsForStudentQueryHandler
    : IQueryHandler<GetSessionDetailsForStudentQuery, List<StudentSessionDetailsResponse>>
{
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IOfferingSessionsRepository _sessionsRepository;
    private readonly ILogger _logger;

    public GetSessionDetailsForStudentQueryHandler(
        IEnrolmentRepository enrolmentRepository,
        IStaffRepository staffRepository,
        IOfferingSessionsRepository sessionsRepository,
        Serilog.ILogger logger)
    {
        _enrolmentRepository = enrolmentRepository;
        _staffRepository = staffRepository;
        _sessionsRepository = sessionsRepository;
        _logger = logger.ForContext<GetSessionDetailsForStudentQuery>();
    }

    public async Task<Result<List<StudentSessionDetailsResponse>>> Handle(GetSessionDetailsForStudentQuery request, CancellationToken cancellationToken)
    {
        List<StudentSessionDetailsResponse> sessionList = new();

        var enrolments = await _enrolmentRepository.GetCurrentByStudentId(request.StudentId, cancellationToken);

        if (enrolments is null || !enrolments.Any())
        {
            _logger.Warning("No active enrolments found for student {id}", request.StudentId);

            return Result.Failure<List<StudentSessionDetailsResponse>>(DomainErrors.Enrolments.Enrolment.NotFoundForStudent(request.StudentId));
        }

        foreach (var enrolment in enrolments)
        {
            var teachers = await _staffRepository.GetPrimaryTeachersForOffering(enrolment.OfferingId, cancellationToken);

            if (teachers is null || !teachers.Any())
            {
                _logger.Warning("Could not find teacher for offering {offering}", enrolment.Offering.Name);
            }

            var sessions = await _sessionsRepository.GetByOfferingId(enrolment.OfferingId, cancellationToken);

            if (sessions is null || !sessions.Any())
            {
                _logger.Warning("Could not find sessions for offering {offering}", enrolment.Offering.Name);
            }
            
            foreach (var session in sessions)
            {
                sessionList.Add(new(
                    session.Period.ToString(),
                    enrolment.Offering.Name,
                    teachers?.Select(teacher => teacher.DisplayName).ToList(),
                    session.Period.Duration));
            }
        }

        return sessionList;
    }
}
