namespace Constellation.Application.Casuals.UpdateCasual;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record UpdateCasualCommand(
    CasualId Id,
    string FirstName,
    string LastName,
    string EmailAddress,
    string SchoolCode,
    string AdobeConnectId)
    : ICommand;