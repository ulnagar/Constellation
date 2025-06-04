namespace Constellation.Application.Domains.Offerings.Commands.RemoveTeacherFromOffering;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.ValueObjects;
using Core.Models.StaffMembers.Identifiers;

public sealed record RemoveTeacherFromOfferingCommand(
    OfferingId OfferingId,
    StaffId StaffId,
    AssignmentType AssignmentType)
    : ICommand;