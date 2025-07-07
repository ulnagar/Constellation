namespace Constellation.Application.Domains.StaffMembers.Queries.GetStaffById;

using Abstractions.Messaging;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Errors;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStaffByIdQueryHandler
    : IQueryHandler<GetStaffByIdQuery, StaffResponse>
{
    private readonly IStaffRepository _staffRepository;
    private readonly ILogger _logger;

    public GetStaffByIdQueryHandler(
        IStaffRepository staffRepository,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _logger = logger.ForContext<GetStaffByIdQuery>();
    }

    public async Task<Result<StaffResponse>> Handle(GetStaffByIdQuery request, CancellationToken cancellationToken)
    {
        StaffMember staffMember = await _staffRepository.GetById(request.StaffId, cancellationToken);

        if (staffMember is null)
        {
            _logger.Warning("Could not find Staff Member with Id {id}", request.StaffId);

            return Result.Failure<StaffResponse>(StaffMemberErrors.NotFound(request.StaffId));
        }
        
        StaffResponse response = new(
            staffMember.Id,
            staffMember.EmployeeId,
            staffMember.Name,
            staffMember.Gender,
            staffMember.EmailAddress,
            staffMember.CurrentAssignment?.SchoolCode,
            staffMember.IsShared);

        return response;
    }
}
