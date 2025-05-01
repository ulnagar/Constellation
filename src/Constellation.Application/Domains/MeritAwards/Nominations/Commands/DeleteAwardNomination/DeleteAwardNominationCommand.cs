namespace Constellation.Application.Awards.DeleteAwardNomination;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record DeleteAwardNominationCommand(
    AwardNominationPeriodId PeriodId,
    AwardNominationId NominationId)
    : ICommand;