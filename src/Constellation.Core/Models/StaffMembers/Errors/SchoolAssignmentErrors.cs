namespace Constellation.Core.Models.StaffMembers.Errors;

using Identifiers;
using Shared;
using System;

public static class SchoolAssignmentErrors
{
    public static readonly Func<SchoolAssignmentId, Error> NotFound = id => new(
        "StaffMember.SchoolAssignment.NotFound",
        $"A School Assignment with Id '{id}' could not be found");
}