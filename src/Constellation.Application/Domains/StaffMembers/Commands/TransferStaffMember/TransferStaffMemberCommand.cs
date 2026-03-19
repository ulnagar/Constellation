namespace Constellation.Application.Domains.StaffMembers.Commands.TransferStaffMember;

using Abstractions.Messaging;
using Core.Models.Identifiers;
using Core.Models.StaffMembers.Identifiers;
using System;

public sealed record TransferStaffMemberCommand(
    StaffId StaffId,
    SchoolCode SchoolCode,
    DateOnly StartDate)
    : ICommand;