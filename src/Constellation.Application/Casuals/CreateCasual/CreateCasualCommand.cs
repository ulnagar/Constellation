namespace Constellation.Application.Casuals.CreateCasual;

using Constellation.Application.Abstractions.Messaging;

public sealed record CreateCasualCommand(
    string FirstName,
    string LastName,
    string EmailAddress,
    string SchoolCode,
    string AdobeConnectId)
    : ICommand;