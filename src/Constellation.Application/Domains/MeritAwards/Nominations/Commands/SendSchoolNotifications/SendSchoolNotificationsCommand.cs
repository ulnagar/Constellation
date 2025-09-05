namespace Constellation.Application.Domains.MeritAwards.Nominations.Commands.SendSchoolNotifications;

using Abstractions.Messaging;
using Core.Models.Awards.Identifiers;
using System;

public sealed record SendSchoolNotificationsCommand(
    AwardNominationPeriodId PeriodId,
    DateOnly DeliveryDate)
    : ICommand;