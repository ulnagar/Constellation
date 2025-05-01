namespace Constellation.Application.Domains.Enrolments.Commands.EnrolMultipleStudentsInOffering;

using Abstractions.Messaging;
using Core.Models.Offerings.Identifiers;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record EnrolMultipleStudentsInOfferingCommand(
    OfferingId OfferingId,
    List<StudentId> StudentIds)
    : ICommand;
