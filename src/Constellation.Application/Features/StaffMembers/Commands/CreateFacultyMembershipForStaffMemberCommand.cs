namespace Constellation.Application.Features.StaffMembers.Commands;

using Constellation.Application.Features.StaffMembers.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public record CreateFacultyMembershipForStaffMemberCommand : IRequest
{
    public string StaffId { get; set; }
    public Guid FacultyId { get; set; }
    public FacultyMembershipRole Role { get; set; }
}

public class CreateFacultyMembershipForStaffMemberCommandHandler : IRequestHandler<CreateFacultyMembershipForStaffMemberCommand>
{
    private readonly IAppDbContext _context;
    private readonly IMediator _mediator;

    public CreateFacultyMembershipForStaffMemberCommandHandler(IAppDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<Unit> Handle(CreateFacultyMembershipForStaffMemberCommand request, CancellationToken cancellationToken)
    {
        // Check if there is an existing (non-deleted) membership to this faculty for this person.
        // If there is and the Role matches? Then ignore request.
        // If there is and the Role doesn't match? (Soft) delete existing record, and create a new one.
        // If there isn't? Create a new one.

        var existingRecords = await _context.Faculties
            .SelectMany(faculty => faculty.Members)
            .Where(member => member.StaffId == request.StaffId && member.FacultyId == request.FacultyId && !member.IsDeleted)
            .ToListAsync(cancellationToken);

        if (existingRecords is not null && existingRecords.Any())
        {
            if (existingRecords.Count > 1)
            {
                // Report error and fail

                return Unit.Value;
            }

            var existingRecord = existingRecords.First();

            if (existingRecord.Role == request.Role)
                return Unit.Value;
            else
                existingRecord.IsDeleted = true;
        }

        var record = new FacultyMembership
        {
            StaffId = request.StaffId,
            FacultyId = request.FacultyId,
            Role = request.Role
        };

        _context.Add(record);

        await _context.SaveChangesAsync(cancellationToken);

        await _mediator.Publish(new StaffFacultyMembershipAddedNotification { Membership = record }, cancellationToken);

        return Unit.Value;
    }
}
