namespace Constellation.Application.StaffMembers.GetStaffLinkedToOffering;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.StaffMembers.Models;
using Constellation.Core.Shared;
using Core.Models.StaffMembers.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStaffLinkedToOfferingQueryHandler
    : IQueryHandler<GetStaffLinkedToOfferingQuery, List<StaffSelectionListResponse>>
{
    private readonly IStaffRepository _staffRepository;

    public GetStaffLinkedToOfferingQueryHandler(IStaffRepository staffRepository)
    {
        _staffRepository = staffRepository;
    }

    public async Task<Result<List<StaffSelectionListResponse>>> Handle(GetStaffLinkedToOfferingQuery request, CancellationToken cancellationToken)
    {
        var staffList = await _staffRepository.GetPrimaryTeachersForOffering(request.OfferingId, cancellationToken);

        if (staffList.Count == 0)
            return new List<StaffSelectionListResponse>();

        return staffList
            .Select(member => 
                new StaffSelectionListResponse(
                    member.StaffId, 
                    member.FirstName, 
                    member.LastName))
            .ToList();
    }
}
