namespace Constellation.Application.StaffMembers.GetLifecycleDetailsForStaffMember;

using Abstractions.Messaging;
using Core.Models;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Students.GetLifecycleDetailsForStudent;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetLifecycleDetailsForStaffMemberQueryHandler
: IQueryHandler<GetLifecycleDetailsForStaffMemberQuery, RecordLifecycleDetailsResponse>
{
    private readonly IStaffRepository _staffRepository;

    public GetLifecycleDetailsForStaffMemberQueryHandler(
        IStaffRepository staffRepository)
    {
        _staffRepository = staffRepository;
    }

    public async Task<Result<RecordLifecycleDetailsResponse>> Handle(GetLifecycleDetailsForStaffMemberQuery request, CancellationToken cancellationToken)
    {
        Staff staffMember = await _staffRepository.GetById(request.StaffId, cancellationToken);

        if (staffMember is null)
        {
            return new RecordLifecycleDetailsResponse(
                string.Empty,
                DateTime.MinValue,
                string.Empty,
                DateTime.MinValue,
                string.Empty,
                null);
        }

        return new RecordLifecycleDetailsResponse(
            string.Empty,
            staffMember.DateEntered ?? DateTime.MinValue,
            string.Empty,
            DateTime.MinValue,
            string.Empty,
            staffMember.DateDeleted ?? DateTime.MinValue);
    }
}
