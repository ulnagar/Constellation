namespace Constellation.Application.Domains.Offerings.Queries.GetSessionDetailsForStudent;

using Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Offerings.ValueObjects;
using Core.Errors;
using Core.Models.Enrolments;
using Core.Models.Enrolments.Repositories;
using Core.Models.Offerings;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Timetables;
using Core.Models.Timetables.Repositories;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetSessionDetailsForStudentQueryHandler
    : IQueryHandler<GetSessionDetailsForStudentQuery, List<StudentSessionDetailsResponse>>
{
    private readonly ITutorialRepository _tutorialRepository;
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IPeriodRepository _periodRepository;
    private readonly ILogger _logger;

    public GetSessionDetailsForStudentQueryHandler(
        ITutorialRepository tutorialRepository,
        IEnrolmentRepository enrolmentRepository,
        IOfferingRepository offeringRepository,
        IStaffRepository staffRepository,
        IPeriodRepository periodRepository,
        ILogger logger)
    {
        _tutorialRepository = tutorialRepository;
        _enrolmentRepository = enrolmentRepository;
        _offeringRepository = offeringRepository;
        _staffRepository = staffRepository;
        _periodRepository = periodRepository;
        _logger = logger.ForContext<GetSessionDetailsForStudentQuery>();
    }

    public async Task<Result<List<StudentSessionDetailsResponse>>> Handle(GetSessionDetailsForStudentQuery request, CancellationToken cancellationToken)
    {
        List<StudentSessionDetailsResponse> sessionList = new();

        List<Enrolment> enrolments = await _enrolmentRepository.GetCurrentByStudentId(request.StudentId, cancellationToken);

        if (enrolments is null || !enrolments.Any())
        {
            _logger.Warning("No active enrolments found for student {id}", request.StudentId);

            return Result.Failure<List<StudentSessionDetailsResponse>>(DomainErrors.Enrolments.Enrolment.NotFoundForStudent(request.StudentId));
        }

        foreach (Enrolment enrolment in enrolments)
        {
            switch (enrolment)
            {
                case OfferingEnrolment offeringEnrolment:
                    {
                        Offering offering = await _offeringRepository.GetById(offeringEnrolment.OfferingId, cancellationToken);

                        if (offering is null)
                        {
                            _logger.Warning("Could not find Offering with Id {id}", offeringEnrolment.OfferingId);

                            continue;
                        }

                        List<TeacherAssignment> assignments = offering
                            .Teachers
                            .Where(assignment =>
                                assignment.Type == AssignmentType.ClassroomTeacher &&
                                !assignment.IsDeleted)
                            .ToList();

                        List<StaffMember> teachers = await _staffRepository.GetListFromIds(assignments.Select(assignment => assignment.StaffId).ToList(), cancellationToken);

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
                                teachers?.Select(teacher => teacher.Name.DisplayName).ToList(),
                                period.Duration));
                        }

                        break;
                    }

                case TutorialEnrolment tutorialEnrolment:
                    {
                        Tutorial tutorial = await _tutorialRepository.GetById(tutorialEnrolment.TutorialId, cancellationToken);

                        if (tutorial is null)
                        {
                            _logger.Warning("Could not find Tutorial with Id {id}", tutorialEnrolment.TutorialId);

                            continue;
                        }

                        List<TutorialSession> sessions = tutorial
                            .Sessions
                            .Where(session => !session.IsDeleted)
                            .ToList();

                        List<StaffMember> teachers = await _staffRepository.GetListFromIds(sessions.Select(session => session.StaffId).ToList(), cancellationToken);

                        foreach (TutorialSession session in sessions)
                        {
                            sessionList.Add(new(
                                session.SortOrder,
                                session.ToString(),
                                tutorial.Name,
                                teachers.Where(teacher => teacher.Id == session.StaffId).Select(teacher => teacher.Name.DisplayName).ToList(),
                                session.Duration));
                        }

                        break;
                    }
            }
        }

        return sessionList;
    }
}
