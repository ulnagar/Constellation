namespace Constellation.Infrastructure.Features.Equipment.Stocktake.Queries;

using Constellation.Application.Features.Equipment.Stocktake.Queries;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.StaffMembers.Models;
using Core.Models;

public class GetStaffForSelectionQueryHandler 
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
        List<StaffSelectionListResponse> response = new();

        List<Staff> staff = await _staffRepository.GetAllActive(cancellationToken);

        foreach (Staff member in staff)
        {
            response.Add(new(
                member.StaffId,
                member.FirstName,
                member.LastName));
        }

        return response;
    }
}