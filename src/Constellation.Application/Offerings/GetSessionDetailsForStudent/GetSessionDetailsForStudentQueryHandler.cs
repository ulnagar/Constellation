﻿namespace Constellation.Application.Offerings.GetSessionDetailsForStudent;

using Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Offerings.ValueObjects;
using Core.Errors;
using Core.Models;
using Core.Models.OfferingEnrolments;
using Core.Models.OfferingEnrolments.Repositories;
using Core.Models.Offerings;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Timetables;
using Core.Models.Timetables.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetSessionDetailsForStudentQueryHandler
    : IQueryHandler<GetSessionDetailsForStudentQuery, List<StudentSessionDetailsResponse>>
{
    private readonly IOfferingEnrolmentRepository _offeringEnrolmentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IPeriodRepository _periodRepository;
    private readonly ILogger _logger;

    public GetSessionDetailsForStudentQueryHandler(
        IOfferingEnrolmentRepository offeringEnrolmentRepository,
        IOfferingRepository offeringRepository,
        IStaffRepository staffRepository,
        IPeriodRepository periodRepository,
        ILogger logger)
    {
        _offeringEnrolmentRepository = offeringEnrolmentRepository;
        _offeringRepository = offeringRepository;
        _staffRepository = staffRepository;
        _periodRepository = periodRepository;
        _logger = logger.ForContext<GetSessionDetailsForStudentQuery>();
    }

    public async Task<Result<List<StudentSessionDetailsResponse>>> Handle(GetSessionDetailsForStudentQuery request, CancellationToken cancellationToken)
    {
        List<StudentSessionDetailsResponse> sessionList = new();

        List<OfferingEnrolment> enrolments = await _offeringEnrolmentRepository.GetCurrentByStudentId(request.StudentId, cancellationToken);

        if (enrolments is null || !enrolments.Any())
        {
            _logger.Warning("No active enrolments found for student {id}", request.StudentId);

            return Result.Failure<List<StudentSessionDetailsResponse>>(DomainErrors.Enrolments.Enrolment.NotFoundForStudent(request.StudentId));
        }

        foreach (OfferingEnrolment enrolment in enrolments)
        {
            Offering offering = await _offeringRepository.GetById(enrolment.OfferingId, cancellationToken);

            if (offering is null)
            {
                _logger.Warning("Could not find Offering with Id {id}", enrolment.OfferingId);

                continue;
            }

            List<TeacherAssignment> assignments = offering
                .Teachers
                .Where(assignment => 
                    assignment.Type == AssignmentType.ClassroomTeacher && 
                    !assignment.IsDeleted)
                .ToList();

            List<Staff> teachers = await _staffRepository.GetListFromIds(assignments.Select(assignment => assignment.StaffId).ToList(), cancellationToken);
            
            foreach (Session session in offering.Sessions.Where(session => !session.IsDeleted))
            {
                Period period = await _periodRepository.GetById(session.PeriodId, cancellationToken);

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
