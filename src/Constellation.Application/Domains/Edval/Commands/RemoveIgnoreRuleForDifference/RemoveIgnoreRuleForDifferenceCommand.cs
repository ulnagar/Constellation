namespace Constellation.Application.Domains.Edval.Commands.RemoveIgnoreRuleForDifference;

using Abstractions.Messaging;
using Core.Models.Edval.Identifiers;

public sealed record RemoveIgnoreRuleForDifferenceCommand(
    DifferenceId DifferenceId)
    : ICommand;