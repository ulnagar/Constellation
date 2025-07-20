namespace Constellation.Application.Domains.Enrolments.Queries.GetStudentEnrolmentsWithDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Errors;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Shared;
using Core.Models.Enrolments;
using Core.Models.Enrolments.Repositories;
using Core.Models.Offerings;
using Core.Models.Offerings.ValueObjects;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Subjects;
using Core.Models.Subjects.Repositories;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStudentEnrolmentsWithDetailsQueryHandler
    : IQueryHandler<GetStudentEnrolmentsWithDetailsQuery, List<StudentEnrolmentResponse>>
{
    private readonly IStaffRepository _staffRepository;
    private readonly ITutorialRepository _tutorialRepository;
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ILogger _logger;

    public GetStudentEnrolmentsWithDetailsQueryHandler(
        IStaffRepository staffRepository,
        ITutorialRepository tutorialRepository,
        IEnrolmentRepository enrolmentRepository,
        IOfferingRepository offeringRepository,
        ICourseRepository courseRepository,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _tutorialRepository = tutorialRepository;
        _enrolmentRepository = enrolmentRepository;
        _offeringRepository = offeringRepository;
        _courseRepository = courseRepository;
        _logger = logger.ForContext<GetStudentEnrolmentsWithDetailsQuery>();
    }

    public async Task<Result<List<StudentEnrolmentResponse>>> Handle(GetStudentEnrolmentsWithDetailsQuery request, CancellationToken cancellationToken)
    {
        List<StudentEnrolmentResponse> returnData = new();

        List<Enrolment> enrolments = await _enrolmentRepository.GetCurrentByStudentId(request.StudentId, cancellationToken);

        if (enrolments is null || !enrolments.Any())
        {
            _logger.Warning("No active enrolments found for student {id}", request.StudentId);

            return Result.Failure<List<StudentEnrolmentResponse>>(DomainErrors.Enrolments.Enrolment.NotFoundForStudent(request.StudentId));
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

                        List<StaffMember> teachers = await _staffRepository.GetPrimaryTeachersForOffering(offeringEnrolment.OfferingId, cancellationToken);

                        if (teachers is null || !teachers.Any())
                        {
                            _logger.Warning("Could not find teacher for offering {offering}", offering.Name);
                        }

                        Course course = await _courseRepository.GetById(offering.CourseId, cancellationToken);

                        if (course is null)
                        {
                            _logger.Warning("Could not find Course with Id {id}", offering.CourseId);

                            continue;
                        }

                        List<StudentEnrolmentResponse.Resource> resources = offering.Resources
                            .Select(entry =>
                                new StudentEnrolmentResponse.Resource(
                                    entry.Type.Value,
                                    entry.Name,
                                    entry.Url))
                            .ToList();

                        returnData.Add(new StudentOfferingEnrolmentResponse(
                            offeringEnrolment.OfferingId,
                            offering.Name,
                            course.Name,
                            teachers?.Select(teacher => teacher.Name.DisplayName).ToList(),
                            resources,
                            false));

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

                        List<StaffId> staffIds = tutorial.Sessions
                            .Where(session => !session.IsDeleted)
                            .Select(session => session.StaffId)
                            .Distinct()
                            .ToList();

                        List<StaffMember> teachers = await _staffRepository.GetListFromIds(staffIds, cancellationToken);

                        if (teachers is null || !teachers.Any())
                        {
                            _logger.Warning("Could not find teacher for tutorial {Tutorial}", tutorial.Name);
                        }

                        List<StudentEnrolmentResponse.Resource> resources = tutorial.Teams
                            .Select(entry =>
                                new StudentEnrolmentResponse.Resource(
                                    ResourceType.MicrosoftTeam.Value,
                                    entry.Name,
                                    entry.Url))
                            .ToList();

                        returnData.Add(new StudentTutorialEnrolmentResponse(
                            tutorial.Id,
                            tutorial.Name,
                            teachers?.Select(teacher => teacher.Name.DisplayName).ToList(),
                            resources,
                            false));

                        break;
                    }
            }
        }

        return returnData;
    }
}
