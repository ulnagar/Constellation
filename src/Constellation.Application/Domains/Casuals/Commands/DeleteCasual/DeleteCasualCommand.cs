namespace Constellation.Application.Domains.Casuals.Commands.DeleteCasual;

using Abstractions.Messaging;
using Core.Models.Identifiers;

public sealed record DeleteCasualCommand(
    CasualId CasualId)
    : ICommand;