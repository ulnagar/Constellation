using Constellation.Application.Features.Partners.Students.Notifications;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Partners.Students.Notifications.StudentWithdrawn
{
    public class WithdrawStudentInDatabase : INotificationHandler<StudentWithdrawnNotification>
    {
        private readonly IAppDbContext _context;
        private readonly ILogger _logger;

        public WithdrawStudentInDatabase(IAppDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Handle(StudentWithdrawnNotification notification, CancellationToken cancellationToken)
        {
            _logger.Information("Attempting to withdraw student with id {studentId}", notification.StudentId);

            var student = await _context.Students
                .FirstOrDefaultAsync(student => student.StudentId == notification.StudentId, cancellationToken);

            if (student == null)
            {
                _logger.Warning("Cannot process student withdrawal ({notification}) as the student cannot be found in the database.", notification);

                return;
            }

            student.IsDeleted = true;
            student.DateDeleted = DateTime.Now;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.Information("Successfully deleted student {student} from database.", student);
        }
    }
}
