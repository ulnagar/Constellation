namespace Constellation.Application.StaffMembers.GetStaffForSelectionList;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.StaffMembers.Models;
using Constellation.Core.Shared;
using Core.Models.StaffMembers.Repositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStaffForSelectionListQueryHandler
    : IQueryHandler<GetStaffForSelectionListQuery, List<StaffSelectionListResponse>>
{
    private readonly IStaffRepository _staffRepository;

    public GetStaffForSelectionListQueryHandler(
        IStaffRepository staffRepository)
    {
        _staffRepository = staffRepository;
    }

    public async Task<Result<List<StaffSelectionListResponse>>> Handle(GetStaffForSelectionListQuery request, CancellationToken cancellationToken)
    {
        List<StaffSelectionListResponse> returnData = new();

        var staff = await _staffRepository.GetAllActive(cancellationToken);

        if (staff.Count == 0)
            return returnData;

        foreach (var member in staff)
        {
            returnData.Add(new StaffSelectionListResponse(
                member.StaffId,
                member.FirstName,
                member.LastName));
        }

        return returnData;
    }
}
