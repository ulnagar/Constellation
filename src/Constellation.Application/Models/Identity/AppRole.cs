namespace Constellation.Application.Models.Identity;

using Microsoft.AspNetCore.Identity;
using System;

public class AppRole : IdentityRole<Guid>
{
    public AppRole() : base() { }

    public AppRole(string name) : base(name) { }
}