namespace Constellation.Application.Offerings.CreateOffering;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.ValueObjects;
using System;

public sealed record CreateOfferingCommand(
    string Name,
    int CourseId,
    DateOnly StartDate,
    DateOnly EndDate)
    : ICommand<OfferingId>;