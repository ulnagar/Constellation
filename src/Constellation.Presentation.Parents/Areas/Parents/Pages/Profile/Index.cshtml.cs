namespace Constellation.Presentation.Parents.Areas.Parents.Pages.Profile;

using Application.Domains.Auth.Queries.GetUserDetails;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.Auth.Queries.GetParentUserDetails;
using Constellation.Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Shared;
using Constellation.Presentation.Parents.Areas.Parents.Models;
using Constellation.Presentation.Shared.Helpers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Extensions;
using Serilog;

[HasPermission(AuthPermission.ParentPortal_View_Value)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForParentPortal();
    }

    [ViewData] public string ActivePage => Models.ActivePage.Profile;

    public ParentUserResponse CurrentUser { get; set; }
    
    public async Task OnGet()
    {
        Result<ParentUserResponse> user = await _mediator.Send(new GetParentUserDetailsQuery(User.GetUserId()));

        if (user.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(user.Error);

            return;
        }

        CurrentUser = user.Value;
    }
}
