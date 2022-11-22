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

namespace Constellation.Infrastructure.Features.Staff.Notifications.StaffFacultyMembershipAdded;

public class UpdateStudentTeamMembershipLevel : INotificationHandler<StaffFacultyMembershipAddedNotification>
{
    private readonly IAppDbContext _context;

    public UpdateStudentTeamMembershipLevel(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(StaffFacultyMembershipAddedNotification notification, CancellationToken cancellationToken)
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
            Action = MSTeamOperationAction.Add,
            DateScheduled = DateTime.Now,
            PermissionLevel = MSTeamOperationPermissionLevel.Owner
        };

        _context.MSTeamOperations.Add(studentTeamOperation);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
