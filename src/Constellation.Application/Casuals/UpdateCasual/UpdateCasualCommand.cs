namespace Constellation.Application.Casuals.UpdateCasual;

using Constellation.Application.Abstractions.Messaging;
using System;

public sealed record UpdateCasualCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string EmailAddress,
    string SchoolCode,
    string AdobeConnectId)
    : ICommand;