namespace Constellation.Application.Domains.Casuals.Commands.UpdateCasual;

using Abstractions.Messaging;
using Core.Models.Identifiers;

public sealed record UpdateCasualCommand(
    CasualId Id,
    string FirstName,
    string LastName,
    string EmailAddress,
    string SchoolCode,
    string AdobeConnectId)
    : ICommand;