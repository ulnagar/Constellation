namespace Constellation.Application.Domains.MeritAwards.Nominations.Commands.DeleteAwardNomination;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Awards.Identifiers;

public sealed record DeleteAwardNominationCommand(
    AwardNominationPeriodId PeriodId,
    AwardNominationId NominationId)
    : ICommand;