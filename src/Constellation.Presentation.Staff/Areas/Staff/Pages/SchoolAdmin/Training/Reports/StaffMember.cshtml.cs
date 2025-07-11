namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Training.Reports;

using Application.Common.PresentationModels;
using Application.Domains.StaffMembers.Queries.GetStaffById;
using Application.Domains.Training.Queries.GetModuleStatusByStaffMember;
using Application.Models.Auth;
using Core.Abstractions.Services;
using Core.Models.StaffMembers.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.CanRunTrainingModuleReports)]
public class StaffMemberModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public StaffMemberModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<StaffMemberModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Training_Reports;
    [ViewData] public string PageTitle => StaffMember is null ? "Staff Training Status" : $"Training Status - {StaffMember.Name.DisplayName}";


    [BindProperty(SupportsGet = true)]
    public StaffId Id { get; set; }

    public List<ModuleStatusResponse> Modules { get; set; } = new();
    public StaffResponse? StaffMember { get; set; }
    
    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve Training report for staff member with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result<StaffResponse> staffRequest = await _mediator.Send(new GetStaffByIdQuery(Id));

        if (staffRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), staffRequest.Error, true)
                .Warning("Failed to retrieve Training report for staff member with id {Id} by user {User}", Id, _currentUserService.UserName);
            
            ModalContent = ErrorDisplay.Create(
                staffRequest.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Reports/Index", values: new { area = "Staff" }));

            return;
        }

        StaffMember = staffRequest.Value;

        Result<List<ModuleStatusResponse>> moduleRequest = await _mediator.Send(new GetModuleStatusByStaffMemberQuery(Id));

        if (moduleRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), moduleRequest.Error, true)
                .Warning("Failed to retrieve Training report for staff member with id {Id} by user {User}", Id, _currentUserService.UserName);
            
            ModalContent = ErrorDisplay.Create(
                moduleRequest.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Reports/Index", values: new { area = "Staff" }));

            return;
        }

        Modules = moduleRequest.Value;
    }
}