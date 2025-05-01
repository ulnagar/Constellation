namespace Constellation.Application.Domains.Casuals.Commands.RestoreCasual;

using Abstractions.Messaging;
using Core.Models.Identifiers;

public sealed record RestoreCasualCommand(
    CasualId CasualId)
    : ICommand;
