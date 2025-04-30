namespace Constellation.Application.StaffMembers.GetStaffMemberNameById;

using Abstractions.Messaging;
using Core.Errors;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStaffMemberNameByIdQueryHandler : IQueryHandler<GetStaffMemberNameByIdQuery, string>
{
    private readonly IStaffRepository _staffRepository;

    public GetStaffMemberNameByIdQueryHandler(
        IStaffRepository staffRepository)
    {
        _staffRepository = staffRepository;
    }

    public async Task<Result<string>> Handle(GetStaffMemberNameByIdQuery request, CancellationToken cancellationToken)
    {
        var staffMember = await _staffRepository.GetById(request.StaffId, cancellationToken);

        if (staffMember is null)
            return Result.Failure<string>(DomainErrors.Partners.Staff.NotFound(request.StaffId));

        return staffMember.DisplayName;
    }
}