namespace Constellation.Application.Domains.ScheduledReports.Commands.CreateScheduledReport;

using Abstractions.Messaging;
using Core.ValueObjects;
using DTOs;

public sealed record CreateScheduledReportCommand<T>(
    T ReportDefinition,
    EmailRecipient Recipient) : ICommand where T : IQuery<FileDto>;