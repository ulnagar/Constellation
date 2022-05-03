using Constellation.Application.Features.Partners.Students.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Partners.Students.Notifications.StudentWithdrawn
{
    public class UpdateAllocatedDeviceStatus : INotificationHandler<StudentWithdrawnNotification>
    {
        private readonly IAppDbContext _context;
        private readonly ILogger _logger;

        public UpdateAllocatedDeviceStatus(IAppDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger.ForContext<StudentWithdrawnNotification>();
        }

        public async Task Handle(StudentWithdrawnNotification notification, CancellationToken cancellationToken)
        {
            _logger.Information("Attempting to update device status for withdrawal of student with id {studentId}", notification.StudentId);

            var devices = await _context.Devices
                .Include(device => device.Allocations)
                .Where(device => device.Allocations.Any(allocation => !allocation.IsDeleted && allocation.StudentId == notification.StudentId))
                .ToListAsync(cancellationToken);

            foreach (var device in devices)
            {
                var allocation = device.Allocations.First(allocation => !allocation.IsDeleted && allocation.StudentId == notification.StudentId);

                device.Status = Core.Enums.Status.RepairingReturning;
                
                var note = new DeviceNotes
                {
                    DateEntered = DateTime.Now,
                    SerialNumber = device.SerialNumber,
                    Details = $"Device unassigned from student due to withdrawal: {allocation.Student.DisplayName}"
                };
                device.Notes.Add(note);

                allocation.IsDeleted = true;
                allocation.DateDeleted = DateTime.Now;

                _logger.Information("Updated device {serial} for withdrawal of student with id {studentId}", device.SerialNumber, notification.StudentId);

                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
