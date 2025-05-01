namespace Constellation.Application.Domains.Casuals.Commands.CreateCasual;

using Abstractions.Messaging;

public sealed record CreateCasualCommand(
    string FirstName,
    string LastName,
    string EmailAddress,
    string SchoolCode,
    string AdobeConnectId)
    : ICommand;