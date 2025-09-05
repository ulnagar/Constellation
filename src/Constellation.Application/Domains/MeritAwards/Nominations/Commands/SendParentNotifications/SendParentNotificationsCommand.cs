namespace Constellation.Application.Domains.MeritAwards.Nominations.Commands.SendParentNotifications;

using Abstractions.Messaging;
using Core.Models.Awards.Identifiers;
using System;

public sealed record SendParentNotificationsCommand(
    AwardNominationPeriodId PeriodId,
    DateOnly DeliveryDate)
    : ICommand;
