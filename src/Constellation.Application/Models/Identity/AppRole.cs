namespace Constellation.Application.Models.Identity;

using Enums;
using Microsoft.AspNetCore.Identity;
using System;

public sealed class AppRole : IdentityRole<Guid>
{
    public const string SuperAdminRole = "SuperAdmin";
    public const string Parent = "Parent";
    public const string Student = "Student";
    public const string Staff = "Staff";

    public AppRole() 
        : base() { }

    public AppRole(string name, AppRoleType type)
        : base(name)
    {
        Type = type;
    }

    public AppRoleType Type { get; private set; } = AppRoleType.None;

    public void UpdateType(AppRoleType type) => Type = type;
}