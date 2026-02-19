namespace Constellation.Application.Domains.LinkedSystems.Canvas.Queries.GetCourseMembershipByCourseCode;

using Abstractions.Messaging;
using AppSettings.Models;
using Core.Enums;
using Core.Errors;
using Core.Models.Canvas.Models;
using Core.Models.Offerings;
using Core.Models.Offerings.Errors;
using Core.Models.Offerings.Repositories;
using Core.Models.Offerings.ValueObjects;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using Core.Models.Students.ValueObjects;
using Core.Shared;
using Extensions;
using Interfaces.Services;
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
    private readonly IAppSettingsService _appSettings;

    public GetCourseMembershipByCourseCodeQueryHandler(
        IOfferingRepository offeringRepository,
        IStudentRepository studentRepository,
        IStaffRepository staffRepository,
        IAppSettingsService appSettings)
    {
        _offeringRepository = offeringRepository;
        _studentRepository = studentRepository;
        _staffRepository = staffRepository;
        _appSettings = appSettings;
    }

    public async Task<Result<List<CanvasCourseMembership>>> Handle(GetCourseMembershipByCourseCodeQuery request, CancellationToken cancellationToken)
    {
        List<CanvasCourseMembership> response = [];

        List<Offering> offerings = await _offeringRepository.GetWithLinkedCanvasResource(request.CourseCode, cancellationToken);

        if (offerings.Count == 0)
        {
            return Result.Failure<List<CanvasCourseMembership>>(OfferingErrors.NotFoundForResource(request.CourseCode.ToString()));
        }

        foreach (Offering offering in offerings.OrderBy(entry => entry.Name))
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
                if (student.StudentReferenceNumber == StudentReferenceNumber.Empty)
                    continue;

                response.Add(new(
                    request.CourseCode, 
                    student.StudentReferenceNumber.Number, 
                    resource.SectionId, 
                    CanvasPermissionLevel.Student));
            }

            // Get Teachers
            List<StaffMember> teachers = await _staffRepository.GetCurrentTeachersForOffering(offering.Id, cancellationToken);

            foreach (StaffMember teacher in teachers)
            {
                response.Add(new(
                    request.CourseCode,
                    teacher.EmployeeId.ToString(),
                    CanvasSectionCode.Empty, 
                    CanvasPermissionLevel.Teacher));
            }

            // Get Head Teachers
            List<StaffMember> headTeachers = await _staffRepository.GetFacultyHeadTeachersForOffering(offering.Id, cancellationToken);

            foreach (StaffMember headTeacher in headTeachers)
            {
                response.Add(new(
                    request.CourseCode,
                    headTeacher.EmployeeId.ToString(),
                    CanvasSectionCode.Empty, 
                    CanvasPermissionLevel.Teacher));
            }
        }

        // Add defined CourseAdmins
        CanvasConfiguration? configuration = await _appSettings.Canvas(cancellationToken);

        if (configuration is null)
        {
            return Result.Failure<List<CanvasCourseMembership>>(ApplicationErrors.InvalidConfiguration(nameof(CanvasConfiguration)));
        }

        Grade? grade = offerings.First().Name.GetGrade();

        if (grade is null)
            return response;

        foreach (var contact in configuration.Admins)
        {
            if (!contact.Value.Contains(grade.Value))
                continue;
            
            response.Add(new(
                request.CourseCode,
                contact.Key.EmployeeId.ToString(),
                CanvasSectionCode.Empty, 
                CanvasPermissionLevel.Teacher));
        }
        
        return response;
    }
}
