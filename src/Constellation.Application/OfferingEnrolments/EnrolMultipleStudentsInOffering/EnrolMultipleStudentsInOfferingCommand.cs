namespace Constellation.Application.OfferingEnrolments.EnrolMultipleStudentsInOffering;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record EnrolMultipleStudentsInOfferingCommand(
    OfferingId OfferingId,
    List<StudentId> StudentIds)
    : ICommand;
