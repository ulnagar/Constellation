namespace Constellation.Application.Domains.StaffMembers.Queries.GetStaffByEmail;

using Abstractions.Messaging;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Models;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStaffByEmailQueryHandler
    : IQueryHandler<GetStaffByEmailQuery, StaffSelectionListResponse>
{
    private readonly IStaffRepository _staffRepository;

    public GetStaffByEmailQueryHandler(
        IStaffRepository staffRepository)
    {
        _staffRepository = staffRepository;
    }

    public async Task<Result<StaffSelectionListResponse>> Handle(GetStaffByEmailQuery request, CancellationToken cancellationToken) 
    {
        StaffMember teacher = await _staffRepository.GetCurrentByEmailAddress(request.EmailAddress, cancellationToken);

        return new StaffSelectionListResponse(teacher.Id, teacher.EmployeeId, teacher.Name);
    }
}
