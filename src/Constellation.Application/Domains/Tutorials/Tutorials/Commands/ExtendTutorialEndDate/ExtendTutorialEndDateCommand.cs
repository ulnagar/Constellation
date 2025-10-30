namespace Constellation.Application.Domains.Tutorials.Tutorials.Commands.ExtendTutorialEndDate;

using Abstractions.Messaging;
using Core.Models.Tutorials.Identifiers;
using System;

public sealed record ExtendTutorialEndDateCommand(
    TutorialId TutorialId,
    DateOnly EndDate)
    : ICommand;
