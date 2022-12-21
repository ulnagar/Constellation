namespace Constellation.Application.GroupTutorials.GetAllTutorials;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAllTutorialsQueryHandler
    : IQueryHandler<GetAllTutorialsQuery, List<GroupTutorialSummaryResponse>>
{
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly ITutorialEnrolmentRepository _tutorialEnrolmentRepository;
    private readonly ITutorialTeacherRepository _tutorialTeacherRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IStudentRepository _studentRepository;

    public GetAllTutorialsQueryHandler(
        IGroupTutorialRepository groupTutorialRepository,
        ITutorialEnrolmentRepository tutorialEnrolmentRepository,
        ITutorialTeacherRepository tutorialTeacherRepository,
        IStaffRepository staffRepository,
        IStudentRepository studentRepository)
    {
        _groupTutorialRepository = groupTutorialRepository;
        _tutorialEnrolmentRepository = tutorialEnrolmentRepository;
        _tutorialTeacherRepository = tutorialTeacherRepository;
        _staffRepository = staffRepository;
        _studentRepository = studentRepository;
    }

    public async Task<Result<List<GroupTutorialSummaryResponse>>> Handle(GetAllTutorialsQuery request, CancellationToken cancellationToken)
    {
        var tutorials = await _groupTutorialRepository.GetAllWithTeachersAndStudents(cancellationToken);

        var summaryList = new List<GroupTutorialSummaryResponse>();

        if (tutorials is null)
        {
            return summaryList;
        }

        foreach (var tutorial in tutorials)
        {
            var teacherLinks = await _tutorialTeacherRepository.GetActiveForTutorial(tutorial.Id, cancellationToken);
            var teachers = await _staffRepository.GetListFromIds(teacherLinks.Select(teacher => teacher.StaffId).ToList(), cancellationToken);

            var studentLinks = await _tutorialEnrolmentRepository.GetActiveForTutorial(tutorial.Id, cancellationToken);
            var students = await _studentRepository.GetListFromIds(studentLinks.Select(student => student.StudentId).ToList(), cancellationToken);

            var entry = new GroupTutorialSummaryResponse(
                tutorial.Id,
                tutorial.Name,
                tutorial.StartDate,
                tutorial.EndDate,
                teachers.Select(teacher => teacher.DisplayName).ToList(),
                students.Select(student => student.DisplayName).ToList());

            summaryList.Add(entry);
        }

        return summaryList;
    }
}
