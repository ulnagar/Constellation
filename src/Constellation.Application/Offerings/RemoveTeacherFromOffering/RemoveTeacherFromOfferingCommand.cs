namespace Constellation.Application.Offerings.RemoveTeacherFromOffering;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.ValueObjects;

public sealed record RemoveTeacherFromOfferingCommand(
    OfferingId OfferingId,
    string StaffId,
    AssignmentType AssignmentType)
    : ICommand;