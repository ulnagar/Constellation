namespace Constellation.Application.Domains.Hosting.Commands.UpsertLivestream;

using Abstractions.Messaging;
using System;

public sealed record UpsertLivestreamCommand(
    Guid? Id,
    string Name,
    string EmbedCode,
    string? Description,
    DateOnly StartsOn,
    DateOnly ExpiresOn)
    : ICommand;