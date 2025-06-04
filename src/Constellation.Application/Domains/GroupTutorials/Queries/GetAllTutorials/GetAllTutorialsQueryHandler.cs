namespace Constellation.Application.Domains.GroupTutorials.Queries.GetAllTutorials;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Shared;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAllTutorialsQueryHandler
    : IQueryHandler<GetAllTutorialsQuery, List<GroupTutorialSummaryResponse>>
{
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly IStaffRepository _staffRepository;

    public GetAllTutorialsQueryHandler(
        IGroupTutorialRepository groupTutorialRepository,
        IStaffRepository staffRepository)
    {
        _groupTutorialRepository = groupTutorialRepository;
        _staffRepository = staffRepository;
    }

    public async Task<Result<List<GroupTutorialSummaryResponse>>> Handle(GetAllTutorialsQuery request, CancellationToken cancellationToken)
    {
        List<GroupTutorialSummaryResponse> response = new();

        List<GroupTutorial> tutorials = request.Filter switch
        {
            GetAllTutorialsQuery.FilterEnum.All => await _groupTutorialRepository.GetAll(cancellationToken),
            GetAllTutorialsQuery.FilterEnum.Active => await _groupTutorialRepository.GetActive(cancellationToken),
            GetAllTutorialsQuery.FilterEnum.Future => await _groupTutorialRepository.GetFuture(cancellationToken),
            GetAllTutorialsQuery.FilterEnum.Inactive => await _groupTutorialRepository.GetInactive(cancellationToken),
            _ => new List<GroupTutorial>()
        };

        if (tutorials.Count == 0)
            return response;

        List<StaffMember> staff = await _staffRepository.GetAll(cancellationToken);
        
        foreach (GroupTutorial tutorial in tutorials)
        {
            List<StaffId> activeStaffIds = tutorial
                .Teachers
                .Where(teacher => !teacher.IsDeleted)
                .Select(teacher => teacher.StaffId)
                .ToList();

            List<StaffMember> teachers = staff.Where(member => activeStaffIds.Contains(member.Id)).ToList();

            GroupTutorialSummaryResponse entry = new(
                tutorial.Id,
                tutorial.Name,
                tutorial.StartDate,
                tutorial.EndDate,
                teachers.Select(teacher => teacher.Name.DisplayName).ToList(),
                tutorial.Enrolments.Count(student => !student.IsDeleted));

            response.Add(entry);
        }
        
        return response;
    }
}
