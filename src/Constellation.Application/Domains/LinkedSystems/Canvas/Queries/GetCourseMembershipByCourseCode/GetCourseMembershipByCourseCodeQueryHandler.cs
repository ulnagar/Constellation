namespace Constellation.Application.Domains.LinkedSystems.Canvas.Queries.GetCourseMembershipByCourseCode;

using Abstractions.Messaging;
using Core.Models.Canvas.Models;
using Core.Models.Offerings;
using Core.Models.Offerings.Errors;
using Core.Models.Offerings.Repositories;
using Core.Models.Offerings.ValueObjects;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using Core.Models.Students.ValueObjects;
using Core.Shared;
using Interfaces.Configuration;
using Microsoft.Extensions.Options;
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
    private readonly CanvasGatewayConfiguration _configuration;

    public GetCourseMembershipByCourseCodeQueryHandler(
        IOfferingRepository offeringRepository,
        IStudentRepository studentRepository,
        IStaffRepository staffRepository,
        IOptions<CanvasGatewayConfiguration> configuration)
    {
        _offeringRepository = offeringRepository;
        _studentRepository = studentRepository;
        _staffRepository = staffRepository;
        _configuration = configuration.Value;
    }

    public async Task<Result<List<CanvasCourseMembership>>> Handle(GetCourseMembershipByCourseCodeQuery request, CancellationToken cancellationToken)
    {
        List<CanvasCourseMembership> response = new();

        List<Offering> offerings = await _offeringRepository.GetWithLinkedCanvasResource(request.CourseCode, cancellationToken);

        if (offerings.Count == 0)
        {
            return Result.Failure<List<CanvasCourseMembership>>(OfferingErrors.NotFoundForResource(request.CourseCode.ToString()));
        }

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
                    teacher.Id.ToString(),
                    CanvasSectionCode.Empty, 
                    CanvasPermissionLevel.Teacher));
            }

            // Get Head Teachers
            List<StaffMember> headTeachers = await _staffRepository.GetFacultyHeadTeachersForOffering(offering.Id, cancellationToken);

            foreach (StaffMember headTeacher in headTeachers)
            {
                response.Add(new(
                    request.CourseCode,
                    headTeacher.Id.ToString(),
                    CanvasSectionCode.Empty, 
                    CanvasPermissionLevel.Teacher));
            }
        }

        // Add defined CourseAdmins
        foreach (StaffId staffId in _configuration.CourseAdmins)
        {
            StaffMember admin = await _staffRepository.GetById(staffId, cancellationToken);

            if (admin is null)
            {
                // TODO: Log Error

                continue;
            }

            response.Add(new(
                request.CourseCode,
                admin.Id.ToString(),
                CanvasSectionCode.Empty, 
                CanvasPermissionLevel.Teacher));
        }
        
        return response;
    }
}
