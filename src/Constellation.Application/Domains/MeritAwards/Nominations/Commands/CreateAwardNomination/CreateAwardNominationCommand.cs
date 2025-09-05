namespace Constellation.Application.Domains.MeritAwards.Nominations.Commands.CreateAwardNomination;

using Abstractions.Messaging;
using Constellation.Core.Models.Awards.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using Core.Models.Students.Identifiers;
using Core.ValueObjects;

public sealed record CreateAwardNominationCommand(
    AwardNominationPeriodId PeriodId,
    AwardType AwardType,
    CourseId CourseId,
    OfferingId OfferingId,
    StudentId StudentId)
    : ICommand;