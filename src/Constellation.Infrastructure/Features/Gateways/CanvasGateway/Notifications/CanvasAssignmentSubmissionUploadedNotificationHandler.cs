using Constellation.Application.Features.Gateways.CanvasGateway.Notifications;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Gateways.CanvasGateway.Notifications
{
    public class CanvasAssignmentSubmissionUploadedNotificationHandler : INotificationHandler<CanvasAssignmentSubmissionUploadedNotification>
    {
        private readonly IAppDbContext _context;
        private readonly ICanvasGateway _gateway;
        private readonly ILogger<ICanvasGateway> _logger;

        public CanvasAssignmentSubmissionUploadedNotificationHandler(IAppDbContext context, ILogger<ICanvasGateway> logger)
        {
            _context = context;
            _logger = logger;
        }

        public CanvasAssignmentSubmissionUploadedNotificationHandler(IAppDbContext context, ICanvasGateway gateway, ILogger<ICanvasGateway> logger)
        {
            _context = context;
            _gateway = gateway;
            _logger = logger;
        }

        public async Task Handle(CanvasAssignmentSubmissionUploadedNotification notification, CancellationToken cancellationToken)
        {
            if (_gateway is null)
            {
                _logger.LogError("Canvas Gateway is not available in this application!");
                return;
            }

            _logger.LogInformation("Starting CanvasAssignmentSubmissionUploaded action for Submission ID {id}", notification.Id);

            var submission = await _context.CanvasAssignmentsSubmissions
                .Include(submission => submission.Assignment)
                .SingleOrDefaultAsync(submission => submission.Id == notification.Id);

            if (submission != null)
                _logger.LogInformation("Found matching submission for assignment {name}", submission.Assignment.Name);
            else
            {
                _logger.LogError("Could not find matching submission for Submission ID {id}", notification.Id);
                return;
            }

            var offering = await _context.Offerings
                .FirstOrDefaultAsync(offering => offering.CourseId == submission.Assignment.CourseId && offering.EndDate >= DateTime.Now);

            if (offering != null)
                _logger.LogInformation("Found matching offering {name} for submission", offering.Name);
            else
            {
                _logger.LogError("Could not find matching offering for Submission ID {id}", notification.Id);
                return;
            }

            var canvasCourseId = $"{offering.EndDate.Year}-{offering.Name.Substring(0, offering.Name.Length - 1)}";

            var file = await _context.StoredFiles
                .SingleOrDefaultAsync(file => file.LinkType == StoredFile.CanvasAssignmentSubmission && file.LinkId == notification.Id.ToString());

            if (file != null)
                _logger.LogInformation("Found matching file {name} for sibmission", file.Name);
            else
            {
                _logger.LogError("Could not find matching file for Submission ID {id}", notification.Id);
                return;
            }

            // Upload file to Canvas
            // Include error checking/retry on failure
            var result = await _gateway.UploadAssignmentSubmission(canvasCourseId, submission.Assignment.CanvasId, submission.StudentId, file);

            if (result)
                _logger.LogInformation("Successfully uploaded submitted file for assignment {name}", submission.Assignment.Name);
            else
            {
                _logger.LogError("Error uploading file for Submission ID {id}", notification.Id);
                return;
            }
        }
    }
}
