namespace Constellation.Application.Offerings.AddTeacherToOffering;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.ValueObjects;

public sealed record AddTeacherToOfferingCommand(
    OfferingId OfferingId,
    string StaffId,
    AssignmentType AssignmentType)
    : ICommand;