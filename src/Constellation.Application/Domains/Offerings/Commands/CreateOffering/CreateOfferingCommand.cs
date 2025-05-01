namespace Constellation.Application.Domains.Offerings.Commands.CreateOffering;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using System;

public sealed record CreateOfferingCommand(
    string Name,
    CourseId CourseId,
    DateOnly StartDate,
    DateOnly EndDate)
    : ICommand<OfferingId>;