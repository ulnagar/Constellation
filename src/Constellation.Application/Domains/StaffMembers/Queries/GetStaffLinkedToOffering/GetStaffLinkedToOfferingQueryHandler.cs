namespace Constellation.Application.Domains.StaffMembers.Queries.GetStaffLinkedToOffering;

using Abstractions.Messaging;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Models;
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
        List<StaffMember> staffList = await _staffRepository.GetPrimaryTeachersForOffering(request.OfferingId, cancellationToken);

        if (staffList.Count == 0)
            return new List<StaffSelectionListResponse>();

        return staffList
            .Select(member => 
                new StaffSelectionListResponse(
                    member.Id, 
                    member.EmployeeId,
                    member.Name))
            .ToList();
    }
}
