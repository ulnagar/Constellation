namespace Constellation.Application.Features.StaffMembers.Commands;

using Constellation.Application.Features.StaffMembers.Notifications;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public record RemoveFacultyMembershipFromStaffMemberCommand : IRequest
{
    public Guid MembershipId { get; init; }
}

public class RemoveFacultyMembershipFromStaffMemberCommandHandler : IRequestHandler<RemoveFacultyMembershipFromStaffMemberCommand>
{
    private readonly IAppDbContext _context;
    private readonly ILogger _logger;
    private readonly IMediator _mediator;

    public RemoveFacultyMembershipFromStaffMemberCommandHandler(IAppDbContext context, ILogger<RemoveFacultyMembershipFromStaffMemberCommand> logger, IMediator mediator)
    {
        _context = context;
        _logger = logger;
        _mediator = mediator;
    }

    public async Task<Unit> Handle(RemoveFacultyMembershipFromStaffMemberCommand request, CancellationToken cancellationToken)
    {
        var record = await _context.Faculties
            .SelectMany(faculty => faculty.Members)
            .Include(member => member.Faculty)
            .Where(member => member.Id == request.MembershipId)
            .SingleOrDefaultAsync(cancellationToken);

        if (record is null)
        {
            // Generate Log Event
            _logger.LogInformation("Could not find FacultyMembership with Id of {id}", request.MembershipId);

            return Unit.Value;
        }

        record.IsDeleted = true;

        await _context.SaveChangesAsync(cancellationToken);

        await _mediator.Publish(new StaffFacultyMembershipRemovedNotification { Membership = record }, cancellationToken);

        return Unit.Value;
    }
}
