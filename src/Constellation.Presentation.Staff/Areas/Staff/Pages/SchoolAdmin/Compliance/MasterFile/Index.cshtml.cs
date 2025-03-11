namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Compliance.MasterFile;

using Constellation.Application.Common.PresentationModels;
using Constellation.Application.ExternalDataConsistency;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Staff.Areas;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.Logging;
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
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Compliance_MasterFile;
    [ViewData] public string PageTitle => "MasterFile Import";

    [BindProperty]
    public IFormFile? FormFile { get; set; }

    [BindProperty]
    public bool EmailReport { get; set; }

    public List<UpdateItem> UpdateItems { get; set; } = new();

    public async Task OnGet() { }

    public async Task<IActionResult> OnPost(CancellationToken cancellationToken)
    {
        _logger.Information("Requested to import and process MasterFile data by user {User}", _currentUserService.UserName);

        if (FormFile is not null)
        {
            try
            {
                string? emailAddress = string.Empty;

                if (EmailReport)
                {
                    emailAddress = User.Identity?.Name;

                    if (emailAddress is null)
                        EmailReport = false;
                }

                await using var target = new MemoryStream();
                await FormFile.CopyToAsync(target, cancellationToken);

                Result<List<UpdateItem>> outputRequest = await _mediator.Send(new MasterFileConsistencyCoordinator(target, EmailReport, emailAddress), cancellationToken);

                if (outputRequest.IsFailure)
                {
                    _logger
                        .ForContext(nameof(Error), outputRequest.Error, true)
                        .Warning("Failed to import and process MasterFile data by user {User}", _currentUserService.UserName);
                    
                    ModalContent = new ErrorDisplay(
                        outputRequest.Error,
                        _linkGenerator.GetPathByPage("/SchoolAdmin/Compliance/MasterFile/Index", values: new { area = "Staff" }));

                    return Page();
                }

                UpdateItems = outputRequest.Value;
            }
            catch (Exception ex)
            {
                _logger
                    .ForContext(nameof(Exception), ex, true)
                    .Warning("Failed to import and process MasterFile data by user {User}", _currentUserService.UserName);

                ModalContent = new ErrorDisplay(
                    new Error("Exception", ex.Message),
                    _linkGenerator.GetPathByPage("/SchoolAdmin/Compliance/MasterFile/Index", values: new { area = "Staff" }));

                return Page();
            }
        }

        return Page();
    }
}