using Microsoft.AspNetCore.Identity;
using System;

namespace Constellation.Application.Models.Identity
{
    public class AppRole : IdentityRole<Guid>
    {
        public AppRole() : base() { }

        public AppRole(string name) : base(name) { }
    }
}
