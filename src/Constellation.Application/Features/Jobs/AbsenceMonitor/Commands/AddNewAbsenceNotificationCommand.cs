using MediatR;
using System;
using System.Collections.Generic;

namespace Constellation.Application.Features.Jobs.AbsenceMonitor.Commands
{
    public class AddNewAbsenceNotificationCommand : IRequest
    {
        public Guid AbsenceId { get; set; }
        public string Type { get; set; }
        public string MessageBody { get; set; }
        public string MessageId { get; set; }
        public ICollection<string> Recipients { get; set; }
    }
}
