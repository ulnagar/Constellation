namespace Constellation.Application.GroupTutorials.GetAllTutorials;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
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
        var watch = System.Diagnostics.Stopwatch.StartNew();

        List<GroupTutorialSummaryResponse> response = new();

        var watch2 = System.Diagnostics.Stopwatch.StartNew();

        List<GroupTutorial> tutorials = request.Filter switch
        {
            GetAllTutorialsQuery.FilterEnum.All => await _groupTutorialRepository.GetAll(cancellationToken),
            GetAllTutorialsQuery.FilterEnum.Active => await _groupTutorialRepository.GetActive(cancellationToken),
            GetAllTutorialsQuery.FilterEnum.Future => await _groupTutorialRepository.GetFuture(cancellationToken),
            GetAllTutorialsQuery.FilterEnum.Inactive => await _groupTutorialRepository.GetInactive(cancellationToken),
            _ => new List<GroupTutorial>()
        };
        watch2.Stop();

        if (tutorials.Count == 0)
            return response;

        List<Staff> staff = await _staffRepository.GetAll(cancellationToken);


        foreach (GroupTutorial tutorial in tutorials)
        {
            List<string> activeStaffIds = tutorial
                .Teachers
                .Where(teacher => !teacher.IsDeleted)
                .Select(teacher => teacher.StaffId)
                .ToList();

            List<Staff> teachers = staff.Where(member => activeStaffIds.Contains(member.StaffId)).ToList();

            GroupTutorialSummaryResponse entry = new(
                tutorial.Id,
                tutorial.Name,
                tutorial.StartDate,
                tutorial.EndDate,
                teachers.Select(teacher => teacher.DisplayName).ToList(),
                tutorial.Enrolments.Count(student => !student.IsDeleted));

            response.Add(entry);
        }

        watch.Stop();

        var time = watch.ElapsedMilliseconds;
        var time2 = watch2.ElapsedMilliseconds;

        return response;
    }
}
