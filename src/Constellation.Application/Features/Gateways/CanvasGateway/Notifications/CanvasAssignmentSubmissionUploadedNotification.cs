using MediatR;
using System;

namespace Constellation.Application.Features.Gateways.CanvasGateway.Notifications
{
    public class CanvasAssignmentSubmissionUploadedNotification : INotification
    {
        public Guid Id { get; set; }
    }
}
