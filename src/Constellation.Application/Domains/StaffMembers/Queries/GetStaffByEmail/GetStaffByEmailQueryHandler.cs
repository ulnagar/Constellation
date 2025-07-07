namespace Constellation.Application.Domains.StaffMembers.Queries.GetStaffByEmail;

using Abstractions.Messaging;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Errors;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Core.ValueObjects;
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
        Result<EmailAddress> emailAddress = EmailAddress.Create(request.EmailAddress);

        StaffMember? teacher = emailAddress.IsSuccess 
            ? await _staffRepository.GetCurrentByEmailAddress(emailAddress.Value, cancellationToken) 
            : null;

        return teacher is null 
            ? Result.Failure<StaffSelectionListResponse>(StaffMemberErrors.NotFoundByEmail(request.EmailAddress)) 
            : new StaffSelectionListResponse(teacher.Id, teacher.EmployeeId, teacher.Name);
    }
}
