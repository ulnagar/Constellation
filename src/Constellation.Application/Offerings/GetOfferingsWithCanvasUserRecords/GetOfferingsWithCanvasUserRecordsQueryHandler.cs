namespace Constellation.Application.Offerings.GetOfferingsWithCanvasUserRecords;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Repositories;
using Constellation.Core.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetOfferingsWithCanvasUserRecordsQueryHandler
    : IQueryHandler<GetOfferingsWithCanvasUserRecordsQuery, List<OfferingSummaryWithUsers>>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IStudentRepository _studentRepository;

    public GetOfferingsWithCanvasUserRecordsQueryHandler(
        IOfferingRepository offeringRepository,
        ICourseRepository courseRepository,
        IStaffRepository staffRepository,
        IStudentRepository studentRepository)
    {
        _offeringRepository = offeringRepository;
        _courseRepository = courseRepository;
        _staffRepository = staffRepository;
        _studentRepository = studentRepository;
    }

    public async Task<Result<List<OfferingSummaryWithUsers>>> Handle(GetOfferingsWithCanvasUserRecordsQuery request, CancellationToken cancellationToken)
    {
        List<OfferingSummaryWithUsers> response = new();
        
        List<Offering> offerings = await _offeringRepository.GetAllActive(cancellationToken);

        List<Course> courses = await _courseRepository.GetAll(cancellationToken);

        List<Staff> staff = await _staffRepository.GetAll(cancellationToken);

        foreach (Offering offering in offerings)
        {
            if (offering.Resources.All(entry => entry.Type != ResourceType.CanvasCourse))
                continue;

            Course course = courses.FirstOrDefault(entry => entry.Id == offering.CourseId);

            if (course is null)
                continue;

            List<string> canvasIds = offering.Resources
                .Where(entry => entry.Type == ResourceType.CanvasCourse)
                .Select(entry => entry.ResourceId)
                .ToList();

            List<OfferingSummaryWithUsers.User> users = new();

            List<string> teacherIds = offering.Teachers
                .Where(entry => !entry.IsDeleted)
                .Select(entry => entry.StaffId)
                .ToList();

            foreach (string staffId in teacherIds)
            {
                Staff teacher = staff.FirstOrDefault(entry => entry.StaffId == staffId);

                if (teacher is null)
                    continue;

                users.Add(new(
                    teacher.StaffId,
                    OfferingSummaryWithUsers.User.UserType.Teacher,
                    OfferingSummaryWithUsers.User.AccessType.Teacher));
            }

            List<Staff> headTeachers = await _staffRepository.GetFacultyHeadTeachersForOffering(offering.Id, cancellationToken);

            foreach (Staff teacher in headTeachers)
            {
                users.Add(new(
                    teacher.StaffId,
                    OfferingSummaryWithUsers.User.UserType.Teacher,
                    OfferingSummaryWithUsers.User.AccessType.Teacher));
            }

            List<Student> students = await _studentRepository.GetCurrentEnrolmentsForOffering(offering.Id, cancellationToken);

            foreach (Student student in students)
            {
                users.Add(new(
                    student.StudentId,
                    OfferingSummaryWithUsers.User.UserType.Student,
                    OfferingSummaryWithUsers.User.AccessType.Student));
            }

            response.Add(new(
                offering.Id,
                offering.Name,
                course.Name,
                offering.StartDate,
                offering.EndDate,
                course.Grade,
                offering.IsCurrent,
                canvasIds,
                users));
        }

        return response;
    }
}
