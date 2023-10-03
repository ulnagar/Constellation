namespace Constellation.Application.Enrolments.EnrolMultipleStudentsInOffering;

using Abstractions.Messaging;
using Core.Models.Offerings.Identifiers;
using System.Collections.Generic;

public sealed record EnrolMultipleStudentsInOfferingCommand(
    OfferingId OfferingId,
    List<string> StudentIds)
    : ICommand;
