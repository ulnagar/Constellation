namespace Constellation.Application.Canvas.GetCourseMembershipByCourseCode;

using Abstractions.Messaging;
using Core.Models;
using Core.Models.Canvas.Models;
using Core.Models.Offerings;
using Core.Models.Offerings.Repositories;
using Core.Models.Offerings.ValueObjects;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using Core.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCourseMembershipByCourseCodeQueryHandler
: IQueryHandler<GetCourseMembershipByCourseCodeQuery, List<CanvasCourseMembership>>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IStaffRepository _staffRepository;

    public GetCourseMembershipByCourseCodeQueryHandler(
        IOfferingRepository offeringRepository,
        IStudentRepository studentRepository,
        IStaffRepository staffRepository)
    {
        _offeringRepository = offeringRepository;
        _studentRepository = studentRepository;
        _staffRepository = staffRepository;
    }

    public async Task<Result<List<CanvasCourseMembership>>> Handle(GetCourseMembershipByCourseCodeQuery request, CancellationToken cancellationToken)
    {
        List<CanvasCourseMembership> response = new();

        List<Offering> offerings = await _offeringRepository.GetWithLinkedCanvasResource(request.CourseCode, cancellationToken);

        foreach (Offering offering in offerings)
        {
            CanvasCourseResource resource = offering.Resources
                .OfType<CanvasCourseResource>()
                .First(resource =>
                    resource.Type == ResourceType.CanvasCourse &&
                    resource.ResourceId == request.CourseCode.ToString());

            // Get Students
            List<Student> students = await _studentRepository.GetCurrentEnrolmentsForOffering(offering.Id, cancellationToken);

            foreach (Student student in students)
            {
                response.Add(new(
                    request.CourseCode, 
                    student.Id.ToString(), 
                    resource.SectionId, 
                    CanvasPermissionLevel.Student));
            }

            // Get Teachers
            List<Staff> teachers = await _staffRepository.GetCurrentTeachersForOffering(offering.Id, cancellationToken);

            foreach (Staff teacher in teachers)
            {
                response.Add(new(
                    request.CourseCode,
                    teacher.StaffId,
                    CanvasSectionCode.Empty, 
                    CanvasPermissionLevel.Teacher));
            }

            // Get Head Teachers
            List<Staff> headTeachers = await _staffRepository.GetFacultyHeadTeachersForOffering(offering.Id, cancellationToken);

            foreach (Staff headTeacher in headTeachers)
            {
                response.Add(new(
                    request.CourseCode,
                    headTeacher.StaffId,
                    CanvasSectionCode.Empty, 
                    CanvasPermissionLevel.Teacher));
            }
        }

        return response;
    }
}
