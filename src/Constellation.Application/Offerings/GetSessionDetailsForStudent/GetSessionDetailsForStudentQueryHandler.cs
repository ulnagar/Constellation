namespace Constellation.Application.Offerings.GetSessionDetailsForStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Shared;
using Core.Models.Enrolments.Repositories;
using Core.Models.StaffMembers.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetSessionDetailsForStudentQueryHandler
    : IQueryHandler<GetSessionDetailsForStudentQuery, List<StudentSessionDetailsResponse>>
{
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ITimetablePeriodRepository _periodRepository;
    private readonly ILogger _logger;

    public GetSessionDetailsForStudentQueryHandler(
        IEnrolmentRepository enrolmentRepository,
        IOfferingRepository offeringRepository,
        IStaffRepository staffRepository,
        ITimetablePeriodRepository periodRepository,
        ILogger logger)
    {
        _enrolmentRepository = enrolmentRepository;
        _offeringRepository = offeringRepository;
        _staffRepository = staffRepository;
        _periodRepository = periodRepository;
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
            var offering = await _offeringRepository.GetById(enrolment.OfferingId, cancellationToken);

            if (offering is null)
            {
                _logger.Warning("Could not find Offering with Id {id}", enrolment.OfferingId);

                continue;
            }

            var assignments = offering
                .Teachers
                .Where(assignment => 
                    assignment.Type == AssignmentType.ClassroomTeacher && 
                    !assignment.IsDeleted)
                .ToList();

            var teachers = await _staffRepository.GetListFromIds(assignments.Select(assignment => assignment.StaffId).ToList(), cancellationToken);
            
            foreach (var session in offering.Sessions.Where(session => !session.IsDeleted))
            {
                var period = await _periodRepository.GetById(session.PeriodId, cancellationToken);

                if (period is null)
                {
                    _logger.Warning("Could not find Period with Id {id}", session.PeriodId);
                    
                    continue;
                }

                sessionList.Add(new(
                    period.SortOrder,
                    period.ToString(),
                    offering.Name,
                    teachers?.Select(teacher => teacher.DisplayName).ToList(),
                    period.Duration));
            }
        }

        return sessionList;
    }
}
