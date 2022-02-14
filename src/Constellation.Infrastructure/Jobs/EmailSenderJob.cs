using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Models;
using Constellation.Infrastructure.DependencyInjection;
using Constellation.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Jobs
{
    public class EmailSenderJob : IEmailSenderJob, IScopedService, IHangfireJob
    {
        private readonly AppDbContext _context;
        private readonly IEmailGateway _emailGateway;

        public EmailSenderJob(AppDbContext context, IEmailGateway emailGateway)
        {
            _context = context;
            _emailGateway = emailGateway;
        }

        public async Task StartJob()
        {
            var emails = await _context.EmailToProcess
                .Where(email => email.Status == EmailToProcess.EmailStatus.Ready)
                .ToListAsync();

            foreach (var email in emails)
            {
                email.FailureMessage = null;

                var message = MimeMessage.Load(email.FilePath);

                try
                {
                    await _emailGateway.Send(message);
                }
                catch (Exception ex)
                {
                    email.FailureCount++;
                    email.FailureMessage = ex.Message;
                }

                if (email.FailureMessage == null)
                {
                    email.Status = EmailToProcess.EmailStatus.Sent;
                    email.SentAt = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
