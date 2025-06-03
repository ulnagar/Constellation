namespace Constellation.Core.Models.StaffMembers.Errors;

using Shared;
using System;

public static class EmployeeIdErrors
{
    public static readonly Error EmptyValue = new(
        "StaffMember.EmployeeId.Empty",
        "An Employee Id must have a value");

    public static readonly Func<string, Error> InvalidValue = id => new(
        "StaffMember.EmployeeId.Invalid",
        $"The provided Employee Id '{id}' is invalid");
}