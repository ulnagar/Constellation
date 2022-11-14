namespace Constellation.Application.Features.MandatoryTraining.Notifications;

using Constellation.Application.DTOs.EmailRequests;
using MediatR;
using System;
using System.Collections.Generic;

public record TrainingExpiringSoonNotification : INotification
{
    // Needs to know details of who to send the email to
    // E.g. Teacher, HT.
    public EmailBaseClass.Recipient Teacher { get; init; }
    public EmailBaseClass.Recipient HeadTeacher { get; init; }

    // Needs to know which courses to include, with the name, expiry frequency, and expiry date.
    // E.g. "Code of Conduct (Every Year)", "25/01/2022"
    public Dictionary<string, DateOnly> ExpiringCertificates { get; init; } = new();
}
