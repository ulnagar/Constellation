namespace Constellation.Application.Domains.Casuals.Commands.CreateCasual;

using Abstractions.Messaging;
using Core.Models.Identifiers;

public sealed record CreateCasualCommand(
    string FirstName,
    string LastName,
    string EmailAddress,
    SchoolCode SchoolCode,
    string EdvalTeacherId)
    : ICommand;