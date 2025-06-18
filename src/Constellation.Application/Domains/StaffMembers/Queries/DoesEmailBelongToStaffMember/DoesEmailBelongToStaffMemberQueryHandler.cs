namespace Constellation.Application.Domains.StaffMembers.Queries.DoesEmailBelongToStaffMember;

using Abstractions.Messaging;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Core.ValueObjects;
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
        Result<EmailAddress> emailAddress = EmailAddress.Create(request.EmailAddress);

        StaffMember? response = emailAddress.IsSuccess
            ? await _staffRepository.GetCurrentByEmailAddress(emailAddress.Value, cancellationToken)
            : null;

        return response is not null;
    }
}
