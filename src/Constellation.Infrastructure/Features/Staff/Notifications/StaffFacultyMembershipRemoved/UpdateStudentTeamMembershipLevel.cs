using Constellation.Application.Enums;
using Constellation.Application.Features.StaffMembers.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Staff.Notifications.StaffFacultyMembershipRemoved;

public class UpdateStudentTeamMembershipLevel : INotificationHandler<StaffFacultyMembershipRemovedNotification>
{
    private readonly IAppDbContext _context;

    public UpdateStudentTeamMembershipLevel(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(StaffFacultyMembershipRemovedNotification notification, CancellationToken cancellationToken)
    {
        if (!notification.Membership.Faculty.Name.Contains("Administration") &&
            !notification.Membership.Faculty.Name.Contains("Executive") &&
            !notification.Membership.Faculty.Name.Contains("Support"))
        {
            return;
        }

        // Create Operation
        var studentTeamOperation = new TeacherEmployedMSTeamOperation()
        {
            StaffId = notification.Membership.StaffId,
            TeamName = MicrosoftTeam.Students,
            Action = MSTeamOperationAction.Demote,
            DateScheduled = DateTime.Now,
            PermissionLevel = MSTeamOperationPermissionLevel.Member
        };

        _context.MSTeamOperations.Add(studentTeamOperation);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
