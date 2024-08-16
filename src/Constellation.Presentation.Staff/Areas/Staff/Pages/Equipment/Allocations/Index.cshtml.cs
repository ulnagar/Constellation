namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Allocations;

using Application.Assets.GetAllocationList;
using Application.Common.PresentationModels;
using Application.Models.Auth;
using Core.Abstractions.Services;
using Core.Models.Assets.Enums;
using Core.Models.Assets.Errors;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;

[Authorize(Policy = AuthPolicies.CanManageAssets)]
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
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Equipment_Assets_Allocations;
    [ViewData] public string PageTitle => "Assets by Allocation";


    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(BaseFromValueBinder))]
    public AllocationType AllocationType { get; set; }
    public List<AllocationListItem> Allocations { get; set; }

    public async Task OnGet()
    {
        AllocationType ??= AllocationType.Student;

        _logger.Information("Requested to retrieve Allocations of type {Type} by {User}", AllocationType.Name, _currentUserService.UserName);

        Result<List<AllocationListItem>> result = AllocationType switch
        {
            _ when AllocationType.Equals(AllocationType.Student) => await _mediator.Send(new GetStudentAllocationListQuery()),
            _ when AllocationType.Equals(AllocationType.Staff) => await _mediator.Send(new GetStaffAllocationListQuery()),
            _ when AllocationType.Equals(AllocationType.School) => await _mediator.Send(new GetSchoolAllocationListQuery()),
            _ when AllocationType.Equals(AllocationType.CommunityMember) => await _mediator.Send(new GetCommunityMemberAllocationListQuery()),
            _ => Result.Failure<List<AllocationListItem>>(AllocationErrors.UnknownType)
        };

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to retrieve Allocations of type {Type} by {User}", AllocationType.Name, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(result.Error);

            return;
        }

        Allocations = result.Value;
    }
}
