using Constellation.Application.Features.Jobs.AbsenceMonitor.Commands;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Jobs.AbsenceMonitor.Commands
{
    public class AddNewAbsenceNotificationCommandHandler : IRequestHandler<AddNewAbsenceNotificationCommand>
    {
        private readonly IAppDbContext _context;

        public AddNewAbsenceNotificationCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(AddNewAbsenceNotificationCommand request, CancellationToken cancellationToken)
        {
            var notification = new AbsenceNotification
            {
                AbsenceId = request.AbsenceId,
                Type = request.Type,
                Message = request.MessageBody,
                SentAt = DateTime.Now,
                Recipients = string.Join(", ", request.Recipients),
                OutgoingId = request.MessageId
            };

            _context.Add(notification);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
