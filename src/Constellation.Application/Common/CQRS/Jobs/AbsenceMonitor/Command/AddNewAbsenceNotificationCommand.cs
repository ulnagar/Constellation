using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Common.CQRS.Jobs.AbsenceMonitor.Command
{
    public class AddNewAbsenceNotificationCommand : IRequest
    {
        public Guid AbsenceId { get; set; }
        public string Type { get; set; }
        public string MessageBody { get; set; }
        public string MessageId { get; set; }
        public ICollection<string> Recipients { get; set; }
    }

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
