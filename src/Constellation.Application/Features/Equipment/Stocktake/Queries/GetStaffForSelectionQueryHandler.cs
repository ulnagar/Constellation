namespace Constellation.Application.Features.Equipment.Stocktake.Queries;

using Constellation.Application.StaffMembers.Models;
using Constellation.Core.Models;
using Core.Models.StaffMembers.Repositories;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStaffForSelectionQueryHandler 
    : IRequestHandler<GetStaffForSelectionQuery, ICollection<StaffSelectionListResponse>>
{
    private readonly IStaffRepository _staffRepository;

    public GetStaffForSelectionQueryHandler(
        IStaffRepository staffRepository)
    {
        _staffRepository = staffRepository;
    }

    public async Task<ICollection<StaffSelectionListResponse>> Handle(GetStaffForSelectionQuery request, CancellationToken cancellationToken)
    {
        List<Staff> staff = await _staffRepository.GetAllActive(cancellationToken);

        List<StaffSelectionListResponse> response = staff.Select(member =>
            new StaffSelectionListResponse(
                member.StaffId,
                member.FirstName,
                member.LastName))
            .ToList();

        return response;
    }
}