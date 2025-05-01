namespace Constellation.Infrastructure.Jobs;

using Application.Abstractions.Messaging;
using Application.Domains.ScheduledReports;
using Application.Domains.ScheduledReports.Models;
using Application.DTOs;
using Application.Interfaces.Jobs;
using Application.Interfaces.Services;
using Core.Abstractions.Clock;
using Core.Shared;
using Microsoft.EntityFrameworkCore;
using Persistence.ConstellationContext;
using System.Net.Mail;

internal sealed class ProcessScheduledReportsJob : IProcessScheduledReportsJob
{
    private readonly AppDbContext _context;
    private readonly ISender _publisher;
    private readonly IEmailService _emailService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public ProcessScheduledReportsJob(
        AppDbContext context,
        ISender publisher,
        IEmailService emailService,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _context = context;
        _publisher = publisher;
        _emailService = emailService;
        _dateTime = dateTime;
        _logger = logger
            .ForContext<ProcessScheduledReportsJob>();
    }

    public async Task StartJob(Guid jobId, CancellationToken cancellationToken)
    {
        ScheduledReport message = await _context
            .Set<ScheduledReport>()
            .Where(report => report.IsCompleted == false)
            .OrderBy(report => report.ScheduledAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (message is null)
            return;

        message.StartRun(_dateTime);

        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            IQuery<FileDto> reportDefinition = message.GetQuery<IQuery<FileDto>>();

            if (reportDefinition is null)
            {
                _logger.Error("Failed to deserialize scheduled report: {@message}", message);

                message.UpdateStatus(Result.Failure(new("Serialization.Deserialize.Failed", "Failed to deserialize scheduled report")));

                await _context.SaveChangesAsync(cancellationToken);

                return;
            }

            Result<FileDto> file = await _publisher.Send(reportDefinition, cancellationToken);

            if (file.IsFailure)
            {
                _logger
                    .ForContext("Query", reportDefinition, true)
                    .ForContext(nameof(Error), file.Error, true)
                    .Error("Failed to process scheduled report with error");

                message.UpdateStatus(file);

                await _context.SaveChangesAsync(cancellationToken);

                return;
            }

            // Email report
            MemoryStream stream = new(file.Value.FileData);
            Attachment attachment = new(stream, file.Value.FileName, file.Value.FileType);

            await _emailService.ForwardCompletedScheduledReport(message.ForwardTo, attachment, cancellationToken);

            attachment.Dispose();
            
            message.UpdateStatus(Result.Success());
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.Error("Failed to run scheduled report: {@message}", e.Message);

            message.UpdateStatus(Result.Failure(new(e.GetType().ToString(), e.Message)));

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}