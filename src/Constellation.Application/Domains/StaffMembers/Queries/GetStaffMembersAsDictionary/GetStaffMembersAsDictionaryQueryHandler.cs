namespace Constellation.Application.Domains.StaffMembers.Queries.GetStaffMembersAsDictionary;

using Abstractions.Messaging;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Errors;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStaffMembersAsDictionaryQueryHandler : IQueryHandler<GetStaffMembersAsDictionaryQuery, Dictionary<StaffId, string>>
{
    private readonly IStaffRepository _staffRepository;

    public GetStaffMembersAsDictionaryQueryHandler(
        IStaffRepository staffRepository)
    {
        _staffRepository = staffRepository;
    }

    public async Task<Result<Dictionary<StaffId, string>>> Handle(GetStaffMembersAsDictionaryQuery request, CancellationToken cancellationToken)
    {
        List<StaffMember> staff = await _staffRepository.GetAllActive(cancellationToken);

        if (staff.Count == 0)
            return Result.Failure<Dictionary<StaffId, string>>(StaffMemberErrors.NoneFound);

        return staff
            .OrderBy(member => member.Name.SortOrder)
            .ToDictionary(member => member.Id, member => member.Name.DisplayName);
    }
}