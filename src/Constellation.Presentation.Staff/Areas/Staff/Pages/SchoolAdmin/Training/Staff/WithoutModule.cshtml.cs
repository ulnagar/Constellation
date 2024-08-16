namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Training.Staff;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Application.Training.GetListOfStaffMembersWithoutModule;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Serilog;

[Authorize(Policy = AuthPolicies.CanEditTrainingModuleContent)]
public class WithoutModuleModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public WithoutModuleModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<WithoutModuleModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Training_Staff;
    [ViewData] public string PageTitle => "Staff Training Dashboard";

    public List<StaffResponse> StaffMembers { get; set; } = new();

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve list of Staff Members without registered Training Module by user {User}", _currentUserService.UserName);

        Result<List<StaffResponse>> response = await _mediator.Send(new GetListOfStaffMembersWithoutModuleQuery());

        if (response.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), response.Error, true)
                .Warning("Failed to retrieve list of Staff Members without registered Training Module by user {User}", _currentUserService.UserName);
            
            ModalContent = new ErrorDisplay(response.Error);

            return;
        }

        StaffMembers = response.Value;
    }
}