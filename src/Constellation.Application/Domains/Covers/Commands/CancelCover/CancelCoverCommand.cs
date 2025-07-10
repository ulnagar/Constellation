namespace Constellation.Application.Domains.Covers.Commands.CancelCover;

using Abstractions.Messaging;
using Constellation.Core.Models.Covers.Identifiers;

public sealed record CancelCoverCommand(
    CoverId CoverId)
    : ICommand;