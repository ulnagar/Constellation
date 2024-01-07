#nullable disable

namespace Constellation.Portal.Parents.Server.Controllers;

using Constellation.Application.Models.Identity;
using Constellation.Application.Parents.GetParentWithStudentIds;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize]
[ApiController]
public class BaseAPIController : ControllerBase
{
    private UserManager<AppUser> _userManager;
    protected UserManager<AppUser> UserManager => _userManager ??= HttpContext.RequestServices.GetService<UserManager<AppUser>>();

    protected async Task<AppUser> GetCurrentUser()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return null;

            var userId = userIdClaim.Value;

            return await UserManager.FindByIdAsync(userId);
        }

        return null;
    }

    protected async Task<bool> HasAuthorizedAccessToStudent(IMediator mediator, string studentId)
    {
        if (string.IsNullOrWhiteSpace(studentId)) 
            return false;

        AppUser user = await GetCurrentUser();

        if (user is null)
            return false;

        Result<List<string>> studentIdRequest = await mediator.Send(new GetParentWithStudentIdsQuery(user.Email));

        if (studentIdRequest.IsFailure)
            return false;

        if (studentIdRequest.Value.Contains(studentId))
            return true;

        return false;
    }
}
