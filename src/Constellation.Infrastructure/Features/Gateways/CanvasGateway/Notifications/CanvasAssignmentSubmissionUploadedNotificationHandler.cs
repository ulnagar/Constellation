using Constellation.Application.Features.Gateways.CanvasGateway.Notifications;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Gateways.CanvasGateway.Notifications
{
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
            var submission = await _context.CanvasAssignmentsSubmissions
                .Include(submission => submission.Assignment)
                .SingleOrDefaultAsync(submission => submission.Id == notification.Id);

            var offering = await _context.Offerings
                .FirstOrDefaultAsync(offering => offering.CourseId == submission.Assignment.CourseId && offering.EndDate >= DateTime.Now);

            var canvasCourseId = $"{offering.EndDate.Year}-{offering.Name.Substring(0, offering.Name.Length - 1)}";

            var file = await _context.StoredFiles
                .SingleOrDefaultAsync(file => file.LinkType == StoredFile.CanvasAssignmentSubmission && file.LinkId == notification.Id.ToString());

            // Upload file to Canvas
            // Include error checking/retry on failure
            var result = _gateway.UploadAssignmentSubmission(canvasCourseId, submission.Assignment.CanvasId, submission.StudentId, file);
        }
    }
}
