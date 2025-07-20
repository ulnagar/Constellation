namespace Constellation.Application.Domains.Tutorials.Queries.GetAllTutorials;

using Abstractions.Messaging;
using Constellation.Core.Models.StaffMembers;
using Constellation.Core.Models.StaffMembers.Identifiers;
using Core.Models.Enrolments;
using Core.Models.Enrolments.Repositories;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students;
using Core.Models.Students.Identifiers;
using Core.Models.Students.Repositories;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Repositories;
using Core.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAllTutorialsQueryHandler
: IQueryHandler<GetAllTutorialsQuery, List<TutorialSummaryResponse>>
{
    private readonly ITutorialRepository _tutorialRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly IStudentRepository _studentRepository;

    public GetAllTutorialsQueryHandler(
        ITutorialRepository tutorialRepository,
        IStaffRepository staffRepository,
        IEnrolmentRepository enrolmentRepository,
        IStudentRepository studentRepository)
    {
        _tutorialRepository = tutorialRepository;
        _staffRepository = staffRepository;
        _enrolmentRepository = enrolmentRepository;
        _studentRepository = studentRepository;
    }

    public async Task<Result<List<TutorialSummaryResponse>>> Handle(GetAllTutorialsQuery request, CancellationToken cancellationToken)
    {
        List<TutorialSummaryResponse> response = new();

        List<Tutorial> tutorials = request.Filter switch
        {
            GetAllTutorialsQuery.FilterEnum.All => await _tutorialRepository.GetAll(cancellationToken),
            GetAllTutorialsQuery.FilterEnum.Active => await _tutorialRepository.GetAllActive(cancellationToken),
            GetAllTutorialsQuery.FilterEnum.Inactive => await _tutorialRepository.GetInactive(cancellationToken),
            _ => new List<Tutorial>()
        };

        if (tutorials.Count == 0)
            return response;

        List<StaffMember> staff = await _staffRepository.GetAll(cancellationToken);
        List<Student> students = await _studentRepository.GetAll(cancellationToken);

        foreach (Tutorial tutorial in tutorials)
        {
            List<StaffId> activeStaffIds = tutorial
                .Sessions
                .Where(session => !session.IsDeleted)
                .Select(session => session.StaffId)
                .ToList();

            List<StaffMember> teachers = staff.Where(member => activeStaffIds.Contains(member.Id)).ToList();

            List<Enrolment> enrolments = await _enrolmentRepository.GetCurrentByTutorialId(tutorial.Id, cancellationToken);

            List<StudentId> studentIds = enrolments
                .Select(enrolment => enrolment.StudentId)
                .ToList();

            List<Student> activeStudents = students.Where(student => studentIds.Contains(student.Id)).ToList();

            TutorialSummaryResponse entry = new(
                tutorial.Id,
                tutorial.Name,
                teachers.Select(teacher => teacher.Name.DisplayName).ToList(),
                activeStudents.Select(student => student.Name.DisplayName).ToList(),
                tutorial.Sessions.Where(session => !session.IsDeleted).Select(session => session.Duration).Sum(),
                tutorial.IsCurrent);

            response.Add(entry);
        }

        return response;
    }
}
