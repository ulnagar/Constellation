namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Locations;

using Application.Common.PresentationModels;
using Application.Domains.AssetManagement.Assets.Queries.GetLocationList;
using Application.Models.Auth;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using Core.Models.Assets.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Extensions;
using Presentation.Shared.Helpers.Attributes;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;

[HasPermission(AuthPermission.Equipment_Assets_View_Value)]
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
            .ForStaffPortal();
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Equipment_Assets_Locations;
    [ViewData] public string PageTitle => "Assets by Location";


    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(BaseFromValueBinder))]
    public LocationCategory Category { get; set; }

    public List<LocationListItem> Locations { get; set; } = new();

    public async Task OnGet()
    {
        Category ??= LocationCategory.PublicSchool;

        _logger
            .Information("Requested to retrieve Assets by location by user {User}", _currentUserService.UserName);

        Result<List<LocationListItem>> result = await _mediator.Send(new GetLocationListQuery(Category));

        if (result.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(result.Error);

            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to retrieve Assets by location by user {User}", _currentUserService.UserName);

            return;
        }

        Locations = result.Value;
    }
}
