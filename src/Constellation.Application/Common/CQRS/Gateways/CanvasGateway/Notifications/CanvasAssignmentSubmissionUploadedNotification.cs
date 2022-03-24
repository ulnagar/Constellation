using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Common.CQRS.Gateways.CanvasGateway.Notifications
{
    public class CanvasAssignmentSubmissionUploadedNotification : INotification
    {
        public Guid Id { get; set; }
    }

    public class CanvasAssignmentSubmissionUploadedNotificationHandler : INotificationHandler<CanvasAssignmentSubmissionUploadedNotification>
    {
        private readonly IAppDbContext _context;
        private readonly ICanvasGateway _gateway;

        public CanvasAssignmentSubmissionUploadedNotificationHandler(IAppDbContext context, ICanvasGateway gateway)
        {
            _context = context;
            _gateway = gateway;
        }

        public async Task Handle(CanvasAssignmentSubmissionUploadedNotification notification, CancellationToken cancellationToken)
        {
            var assignment = await _context.CanvasAssignmentsSubmissions
                .SingleOrDefaultAsync(submission => submission.Id == notification.Id);

            var file = await _context.StoredFiles
                .SingleOrDefaultAsync(file => file.LinkType == StoredFile.CanvasAssignmentSubmission && file.LinkId == notification.Id.ToString());

            // Upload file to Canvas
            // Include error checking/retry on failure
        }
    }
}
