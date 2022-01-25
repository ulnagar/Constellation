using Microsoft.AspNetCore.Authorization;
using System;

namespace Constellation.Presentation.Server.Helpers.Attributes
{
    public class RolesAttribute : AuthorizeAttribute
    {
        public RolesAttribute(params string[] roles)
        {
            Roles = String.Join(",", roles);
        }
    }
}
