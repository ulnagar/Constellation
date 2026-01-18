namespace Constellation.Application.Models.Identity.Errors;

using Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class AuthErrors
{
    public static Func<Guid, Error> RoleNotFound = roleId =>
        new Error(
            "Auth.Role.NotFound",
            $"The role with ID '{roleId}' was not found.");
}
