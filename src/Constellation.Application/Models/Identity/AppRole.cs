namespace Constellation.Application.Models.Identity;

using Enums;
using Microsoft.AspNetCore.Identity;
using System;

public sealed class AppRole : IdentityRole<Guid>
{
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