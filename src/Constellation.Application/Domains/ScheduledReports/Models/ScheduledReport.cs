#nullable enable
namespace Constellation.Application.Domains.ScheduledReports.Models;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Shared;
using Core.ValueObjects;
using DTOs;
using Identifiers;
using Newtonsoft.Json;
using System;

public sealed class ScheduledReport
{
    public ScheduledReportId Id { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public EmailRecipient ForwardTo { get; private set; } = EmailRecipient.NoReply;
    public string ReportDefinition { get; init; } = string.Empty;
    public DateTime? LastRan { get; private set; }
    public Result? LastResult { get; private set; }
    public bool IsCompleted { get; private set; }

    public static ScheduledReport Create<T>(IDateTimeProvider dateTime, T reportDefinition, EmailRecipient recipient) where T : IQuery<FileDto>
    {
        ScheduledReport message = new()
        {
            Id = new(),
            ScheduledAt = dateTime.Now,
            ReportDefinition = JsonConvert.SerializeObject(
                reportDefinition,
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }),
            ForwardTo = recipient
        };

        return message;
    }

    public T GetQuery<T>() =>
        JsonConvert
            .DeserializeObject<T>(
                ReportDefinition,
                new JsonSerializerSettings
                {
                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                    TypeNameHandling = TypeNameHandling.All
                });

    public void StartRun(IDateTimeProvider dateTime)
    {
        LastRan = dateTime.Now;
        LastResult = null;
    }

    public void UpdateStatus(Result result)
    {
        LastResult = result;

        if (result.IsSuccess)
            IsCompleted = true;
    }
}
