namespace Constellation.Application.Domains.StaffMembers.Queries.GetLifecycleDetailsForStaffMember;

using Abstractions.Messaging;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Students.Queries.GetLifecycleDetailsForStudent;
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
        StaffMember staffMember = await _staffRepository.GetById(request.StaffId, cancellationToken);

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
            staffMember.CreatedBy,
            staffMember.CreatedAt,
            staffMember.ModifiedBy,
            staffMember.ModifiedAt,
            staffMember.DeletedBy,
            staffMember.DeletedAt);
    }
}
