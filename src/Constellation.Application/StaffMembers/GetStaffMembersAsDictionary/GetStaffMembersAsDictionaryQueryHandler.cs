namespace Constellation.Application.StaffMembers.GetStaffMembersAsDictionary;

using Abstractions.Messaging;
using Core.Errors;
using Core.Models;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStaffMembersAsDictionaryQueryHandler : IQueryHandler<GetStaffMembersAsDictionaryQuery, Dictionary<string, string>>
{
    private readonly IStaffRepository _staffRepository;

    public GetStaffMembersAsDictionaryQueryHandler(
        IStaffRepository staffRepository)
    {
        _staffRepository = staffRepository;
    }

    public async Task<Result<Dictionary<string, string>>> Handle(GetStaffMembersAsDictionaryQuery request, CancellationToken cancellationToken)
    {
        List<Staff> staff = await _staffRepository.GetAllActive(cancellationToken);

        if (staff.Count == 0)
            return Result.Failure<Dictionary<string, string>>(DomainErrors.Partners.Staff.NoneFound);

        return staff
            .OrderBy(staff => staff.LastName)
            .ToDictionary(staff => staff.StaffId, staff => $"{staff.FirstName} {staff.LastName}");
    }
}