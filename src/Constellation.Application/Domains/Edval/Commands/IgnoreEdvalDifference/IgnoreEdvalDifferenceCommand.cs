namespace Constellation.Application.Domains.Edval.Commands.IgnoreEdvalDifference;

using Abstractions.Messaging;
using Core.Models.Edval.Identifiers;

public sealed record IgnoreEdvalDifferenceCommand(
    DifferenceId DifferenceId)
    : ICommand;