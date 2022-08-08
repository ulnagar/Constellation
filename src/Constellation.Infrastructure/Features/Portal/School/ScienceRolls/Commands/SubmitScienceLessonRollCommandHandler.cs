using Constellation.Application.Features.Lessons.Notifications;
using Constellation.Application.Features.Portal.School.ScienceRolls.Commands;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Portal.School.ScienceRolls.Commands
{
    public class SubmitScienceLessonRollCommandHandler : IRequestHandler<SubmitScienceLessonRollCommand>
    {
        private readonly IAppDbContext _context;
        private readonly IMediator _mediator;

        public SubmitScienceLessonRollCommandHandler(IAppDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<Unit> Handle(SubmitScienceLessonRollCommand request, CancellationToken cancellationToken)
        {
            var schoolContact = await _context.SchoolContacts
                .FirstOrDefaultAsync(contact => contact.EmailAddress == request.UserEmail, cancellationToken);

            var roll = await _context.LessonRolls
                .Include(roll => roll.Attendance)
                .FirstOrDefaultAsync(roll => roll.Id == request.RollId, cancellationToken);

            if (schoolContact == null || roll == null)
                return Unit.Value;

            roll.LessonDate = request.LessonDate;
            roll.SubmittedDate = DateTime.Today;
            roll.SchoolContactId = schoolContact.Id;
            roll.Comment = request.Comment;
            roll.Status = LessonStatus.Completed;

            foreach (var attendanceRecord in roll.Attendance)
            {
                var vmRecord = request.Attendance.FirstOrDefault(attendance => attendance.Key == attendanceRecord.Id);

                if (vmRecord.Key != new Guid())
                {
                    if (attendanceRecord.Present == vmRecord.Value)
                        continue;

                    attendanceRecord.Present = vmRecord.Value;

                    if (vmRecord.Value)
                    {
                        await _mediator.Publish(new StudentMarkedPresentInScienceLessonNotification { RollId = attendanceRecord.LessonRollId, StudentId = attendanceRecord.StudentId }, cancellationToken);
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
