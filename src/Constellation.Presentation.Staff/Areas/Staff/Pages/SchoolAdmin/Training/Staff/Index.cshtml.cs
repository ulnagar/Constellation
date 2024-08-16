namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Training.Staff;

using Application.Common.PresentationModels;
using Constellation.Application.Models.Auth;
using Constellation.Application.Training.GetModuleStatusByStaffMember;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
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

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Training_Staff;
    [ViewData] public string PageTitle => "Staff Training Dashboard";


    [BindProperty(SupportsGet = true)]
    public string StaffId { get; set; }

    public List<ModuleStatusResponse> Modules { get; set; }

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve Training Completion details for staff member with id {Id} by user {User}", StaffId, _currentUserService.UserName);

        Result<List<ModuleStatusResponse>> completionRequest = await _mediator.Send(new GetModuleStatusByStaffMemberQuery(StaffId));

        if (completionRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), completionRequest.Error, true)
                .Warning("Failed to retrieve Training Completion details for staff member with id {Id} by user {User}", StaffId, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                completionRequest.Error,
                _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Staff" }));

            return;
        }

        Modules = completionRequest.Value;
    }
}
