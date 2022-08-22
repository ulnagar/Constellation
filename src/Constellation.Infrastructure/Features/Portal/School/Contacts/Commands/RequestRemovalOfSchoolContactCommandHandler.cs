using Constellation.Application.Features.Portal.School.Contacts.Commands;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Portal.School.Contacts.Commands
{
    public class RequestRemovalOfSchoolContactCommandHandler : IRequestHandler<RequestRemovalOfSchoolContactCommand>
    {
        private readonly IAppDbContext _context;
        private readonly IEmailGateway _emailSender;
        private readonly IRazorViewToStringRenderer _razorService;

        public RequestRemovalOfSchoolContactCommandHandler(IAppDbContext context, IEmailGateway emailSender,
            IRazorViewToStringRenderer razorService)
        {
            _context = context;
            _emailSender = emailSender;
            _razorService = razorService;
        }

        public async Task<Unit> Handle(RequestRemovalOfSchoolContactCommand request, CancellationToken cancellationToken)
        {
            var contactAssignment = await _context.SchoolContactRoles
                .Include(role => role.SchoolContact)
                .Include(role => role.School)
                .FirstOrDefaultAsync(role => role.Id == request.AssignmentId, cancellationToken);

            // Send email to school requesting removal
            var viewModel = "<p>A school contact change has been requested:</p>";
            viewModel += $"<p><strong>{contactAssignment.SchoolContact.DisplayName}</strong> should be removed as <strong>{contactAssignment.Role}</strong> at <strong>{contactAssignment.School.Name}</strong></p>";
            viewModel += $"<p>This change was requested by <strong>{request.CancelledBy}</strong> on <strong>{request.CancelledAt.ToLongDateString()}</strong> with the comment:</p>";
            viewModel += $"<p><strong>{request.Comment}<strong></p>";

            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/PlainEmail.cshtml", viewModel);

            var toRecipients = new Dictionary<string, string>
            {
                { "Aurora College IT Support", "support@aurora.nsw.edu.au" },
                { "Aurora College", "auroracoll-h.school@det.nsw.edu.au" }
            };

            await _emailSender.Send(toRecipients, "noreply@aurora.nsw.edu.au", "School Contact change requested", body);

            return Unit.Value;
        }
    }
}
