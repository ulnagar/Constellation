namespace Constellation.Presentation.Server.Areas.Admin.Pages.Auth.Users;

using Application.Common.PresentationModels;
using Application.Domains.Auth.Commands.UpdateUser;
using Application.Domains.Auth.Queries.GetUserDetails;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Server.BaseModels;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Helpers.Attributes;
using System.ComponentModel.DataAnnotations;

[HasPermission(AuthPermission.Admin_Authentication_Edit_Value)]
public class EditModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public EditModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Models.ActivePage.Auth_Users;
    [ViewData] public string PageTitle => "Auth Users";

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public string FirstName { get; set; }

    [BindProperty]
    public string LastName { get; set; }

    [BindProperty]
    [EmailAddress]
    public string Email { get; set; }

    public async Task OnGet()
    {
        Result<UserResponse> user = await _mediator.Send(new GetUserDetailsQuery(Id));

        if (user.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                user.Error,
                _linkGenerator.GetPathByPage("/Auth/Users/Index", values: new { area = "Admin" }));

            return;
        }

        FirstName = user.Value.Name.FirstName;
        LastName = user.Value.Name.LastName;
        Email = user.Value.Email;
    }

    public async Task<IActionResult> OnPost()
    {
        Result update = await _mediator.Send(new UpdateUserCommand(Id, FirstName, LastName, Email));

        if (update.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                update.Error,
                _linkGenerator.GetPathByPage("/Auth/Users/Index", values: new { area = "Admin" }));

            return Page();
        }

        return RedirectToPage("/Auth/Users/Details", routeValues: new { area = "Admin", Id });
    }
}
