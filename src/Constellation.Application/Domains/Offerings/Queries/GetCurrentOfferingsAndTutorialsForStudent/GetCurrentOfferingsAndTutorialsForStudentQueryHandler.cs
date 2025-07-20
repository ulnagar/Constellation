namespace Constellation.Application.Domains.Offerings.Queries.GetCurrentOfferingsAndTutorialsForStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.Enrolments.Repositories;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Models.StaffMembers;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Repositories;
using Constellation.Core.Models.Tutorials;
using Constellation.Core.Models.Tutorials.Repositories;
using Constellation.Core.Shared;
using Core.Models.StaffMembers.Identifiers;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCurrentOfferingsAndTutorialsForStudentQueryHandler
: IQueryHandler<GetCurrentOfferingsAndTutorialsForStudentQuery, List<DetailResponse>>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly ITutorialRepository _tutorialRepository;
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ILogger _logger;

    public GetCurrentOfferingsAndTutorialsForStudentQueryHandler(
        IOfferingRepository offeringRepository,
        ITutorialRepository tutorialRepository,
        IEnrolmentRepository enrolmentRepository,
        IStaffRepository staffRepository,
        ICourseRepository courseRepository,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _tutorialRepository = tutorialRepository;
        _enrolmentRepository = enrolmentRepository;
        _staffRepository = staffRepository;
        _courseRepository = courseRepository;
        _logger = logger
            .ForContext<GetCurrentOfferingsAndTutorialsForStudentQuery>();
    }

    public async Task<Result<List<DetailResponse>>> Handle(GetCurrentOfferingsAndTutorialsForStudentQuery request, CancellationToken cancellationToken)
    {
        List<DetailResponse> response = new();

        List<Enrolment> enrolments = await _enrolmentRepository.GetCurrentByStudentId(request.StudentId, cancellationToken);

        List<StaffMember> staff = await _staffRepository.GetAllActive(cancellationToken);

        foreach (Enrolment enrolment in enrolments)
        {
            switch (enrolment)
            {
                case OfferingEnrolment offeringEnrolment:
                    {
                        Offering offering = await _offeringRepository.GetById(offeringEnrolment.OfferingId, cancellationToken);

                        if (offering is null)
                            continue;

                        Course course = await _courseRepository.GetById(offering.CourseId, cancellationToken);

                        if (course is null)
                            continue;

                        List<DetailResponse.Teacher> teachers = new();

                        foreach (TeacherAssignment teacher in offering.Teachers.Where(entry => entry.Type == AssignmentType.ClassroomTeacher && !entry.IsDeleted))
                        {
                            StaffMember staffMember = staff.FirstOrDefault(entry => entry.Id == teacher.StaffId);

                            if (staffMember is null)
                                continue;

                            teachers.Add(new(staffMember.Name, staffMember.EmailAddress));
                        }

                        List<DetailResponse.Resource> resources = new();

                        foreach (Resource resource in offering.Resources)
                        {
                            resources.Add(new(
                                resource.Type.Value,
                                resource.Name,
                                resource.Url));
                        }

                        response.Add(new(
                            offering.Name,
                            course.Name,
                            teachers,
                            resources));

                        break;
                    }

                case TutorialEnrolment tutorialEnrolment:
                    {
                        Tutorial tutorial = await _tutorialRepository.GetById(tutorialEnrolment.TutorialId, cancellationToken);

                        if (tutorial is null)
                            continue;

                        List<DetailResponse.Teacher> teachers = [];

                        List<StaffId> staffIds = tutorial.Sessions
                            .Where(session => !session.IsDeleted)
                            .Select(session => session.StaffId)
                            .Distinct()
                            .ToList();

                        foreach (StaffId staffId in staffIds)
                        {
                            StaffMember staffMember = staff.FirstOrDefault(entry => entry.Id == staffId);

                            if (staffMember is null)
                                continue;

                            teachers.Add(new(staffMember.Name, staffMember.EmailAddress));
                        }

                        List<DetailResponse.Resource> resources = [];

                        foreach (TeamsResource resource in tutorial.Teams)
                        {
                            resources.Add(new(
                                ResourceType.MicrosoftTeam.Value,
                                resource.Name,
                                resource.Url));
                        }

                        response.Add(new (
                            tutorial.Name,
                            "Tutorial",
                            teachers,
                            resources));

                        break;
                    }
            }
        }

        return response;
    }
}
