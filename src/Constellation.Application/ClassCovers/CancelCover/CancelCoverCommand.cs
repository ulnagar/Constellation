namespace Constellation.Application.ClassCovers.CancelCover;

using Constellation.Application.Abstractions.Messaging;
using System;

public sealed record CancelCoverCommand(
    Guid CoverId)
    : ICommand;