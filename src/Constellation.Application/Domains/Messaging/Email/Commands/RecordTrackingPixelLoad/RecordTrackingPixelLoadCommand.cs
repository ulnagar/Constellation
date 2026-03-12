namespace Constellation.Application.Domains.Messaging.Email.Commands.RecordTrackingPixelLoad;

using Abstractions.Messaging;
using Core.Models.Messaging.Email.Identifiers;
using System;

public sealed record RecordTrackingPixelLoadCommand(
    EmailId EmailId,
    DateTimeOffset OpenedAt,
    string IPAddress,
    string UserAgent)
    : ICommand;
