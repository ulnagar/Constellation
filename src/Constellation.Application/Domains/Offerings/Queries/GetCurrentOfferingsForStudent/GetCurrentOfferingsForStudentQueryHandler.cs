namespace Constellation.Application.Domains.Offerings.Queries.GetCurrentOfferingsForStudent;

using Abstractions.Messaging;
using Core.Models.Enrolments;
using Core.Models.Enrolments.Repositories;
using Core.Models.Offerings;
using Core.Models.Offerings.Repositories;
using Core.Models.Offerings.ValueObjects;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Subjects;
using Core.Models.Subjects.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCurrentOfferingsForStudentQueryHandler
: IQueryHandler<GetCurrentOfferingsForStudentQuery, List<OfferingDetailResponse>>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ILogger _logger;

    public GetCurrentOfferingsForStudentQueryHandler(
        IOfferingRepository offeringRepository,
        IEnrolmentRepository enrolmentRepository,
        IStaffRepository staffRepository,
        ICourseRepository courseRepository,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _enrolmentRepository = enrolmentRepository;
        _staffRepository = staffRepository;
        _courseRepository = courseRepository;
        _logger = logger
            .ForContext<GetCurrentOfferingsForStudentQuery>();
    }

    public async Task<Result<List<OfferingDetailResponse>>> Handle(GetCurrentOfferingsForStudentQuery request, CancellationToken cancellationToken)
    {
        List<OfferingDetailResponse> response = new();

        List<Enrolment> enrolments = await _enrolmentRepository.GetCurrentByStudentId(request.StudentId, cancellationToken);

        List<StaffMember> staff = await _staffRepository.GetAllActive(cancellationToken);

        foreach (Enrolment enrolment in enrolments)
        {
            Offering offering = await _offeringRepository.GetById(enrolment.OfferingId, cancellationToken);

            if (offering is null)
                continue;

            Course course = await _courseRepository.GetById(offering.CourseId, cancellationToken);

            if (course is null)
                continue;

            List<OfferingDetailResponse.Teacher> teachers = new();

            foreach (TeacherAssignment teacher in offering.Teachers.Where(entry => entry.Type == AssignmentType.ClassroomTeacher && !entry.IsDeleted))
            {
                StaffMember staffMember = staff.FirstOrDefault(entry => entry.Id == teacher.StaffId);

                if (staffMember is null)
                    continue;
                
                teachers.Add(new(staffMember.Name, staffMember.EmailAddress));
            }

            List<OfferingDetailResponse.Resource> resources = new();

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
        }

        return response;
    }
}
