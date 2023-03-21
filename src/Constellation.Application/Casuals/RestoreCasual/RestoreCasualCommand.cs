namespace Constellation.Application.Casuals.RestoreCasual;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record RestoreCasualCommand(
    CasualId CasualId)
    : ICommand;
