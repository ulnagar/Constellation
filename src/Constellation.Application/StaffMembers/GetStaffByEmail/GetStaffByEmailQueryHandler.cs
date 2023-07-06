namespace Constellation.Application.StaffMembers.GetStaffByEmail;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.StaffMembers.Models;
using Constellation.Core.Models;
using Constellation.Core.Shared;
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
        Staff teacher = await _staffRepository.GetByEmailAddress(request.EmailAddress, cancellationToken);

        return new StaffSelectionListResponse(teacher.StaffId, teacher.FirstName, teacher.LastName);
    }
}
