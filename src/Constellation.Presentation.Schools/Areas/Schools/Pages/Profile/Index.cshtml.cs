namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Profile;

using Application.Domains.Auth.Queries.GetParentUserDetails;
using Application.Domains.Auth.Queries.GetSchoolContactUserDetails;
using Application.Models.Auth;
using Constellation.Application.Common.PresentationModels;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Extensions;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Attributes;
using Serilog;

[HasPermission(AuthPermission.SchoolsPortal_View_Value)]
public class IndexModel : BasePageModel
{
    private ISender _mediator => Mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    [ViewData] public string ActivePage => Models.ActivePage.Profile;

    public ContactUserResponse CurrentUser { get; set; }

    public async Task OnGet()
    {
        Result<ContactUserResponse> user = await _mediator.Send(new GetSchoolContactUserDetailsQuery(User.GetUserId()));

        if (user.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(user.Error);

            return;
        }

        CurrentUser = user.Value;
    }
}