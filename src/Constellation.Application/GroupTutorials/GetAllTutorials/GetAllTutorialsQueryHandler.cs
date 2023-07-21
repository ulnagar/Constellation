namespace Constellation.Application.GroupTutorials.GetAllTutorials;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Models;
using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAllTutorialsQueryHandler
    : IQueryHandler<GetAllTutorialsQuery, List<GroupTutorialSummaryResponse>>
{
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IStudentRepository _studentRepository;

    public GetAllTutorialsQueryHandler(
        IGroupTutorialRepository groupTutorialRepository,
        IStaffRepository staffRepository,
        IStudentRepository studentRepository)
    {
        _groupTutorialRepository = groupTutorialRepository;
        _staffRepository = staffRepository;
        _studentRepository = studentRepository;
    }

    public async Task<Result<List<GroupTutorialSummaryResponse>>> Handle(GetAllTutorialsQuery request, CancellationToken cancellationToken)
    {
        List<GroupTutorial> tutorials = await _groupTutorialRepository.GetAll(cancellationToken);

        List<GroupTutorialSummaryResponse> summaryList = new();

        if (tutorials is null)
            return summaryList;

        foreach (GroupTutorial tutorial in tutorials)
        {
            List<Staff> teachers = await _staffRepository.GetListFromIds(tutorial.Teachers.Select(teacher => teacher.StaffId).ToList(), cancellationToken);

            List<Student> students = await _studentRepository.GetListFromIds(tutorial.Enrolments.Select(student => student.StudentId).ToList(), cancellationToken);

            GroupTutorialSummaryResponse entry = new(
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
