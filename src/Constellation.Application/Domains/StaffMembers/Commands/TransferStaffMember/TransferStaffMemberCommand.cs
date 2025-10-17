namespace Constellation.Application.Domains.StaffMembers.Commands.TransferStaffMember;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;
using System;

public sealed record TransferStaffMemberCommand(
    StaffId StaffId,
    string SchoolCode,
    DateOnly StartDate)
    : ICommand;