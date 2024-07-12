namespace Constellation.Application.StaffMembers.DoesEmailBelongToStaffMember;

using Abstractions.Messaging;
using Core.Models;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class DoesEmailBelongToStaffMemberQueryHandler
: IQueryHandler<DoesEmailBelongToStaffMemberQuery, bool>
{
    private readonly IStaffRepository _staffRepository;
    private readonly ILogger _logger;

    public DoesEmailBelongToStaffMemberQueryHandler(
        IStaffRepository staffRepository,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _logger = logger.ForContext<DoesEmailBelongToStaffMemberQuery>();
    }

    public async Task<Result<bool>> Handle(DoesEmailBelongToStaffMemberQuery request, CancellationToken cancellationToken)
    {
        Staff? response = await _staffRepository.GetCurrentByEmailAddress(request.EmailAddress, cancellationToken);

        return response is not null;
    }
}
