namespace Constellation.Core.Models.Offerings.Errors;

using Identifiers;
using Shared;
using System;

public static class TeacherAssignmentErrors
{
    public static readonly Func<AssignmentId, Error> NotFound = id => new(
        "Offerings.TeacherAssignment.NotFound",
        $"Could not find Teacher Assignment with Id {id}");
}
