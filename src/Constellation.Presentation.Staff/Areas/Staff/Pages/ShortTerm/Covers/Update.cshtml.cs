namespace Constellation.Presentation.Staff.Areas.Staff.Pages.ShortTerm.Covers;

using Application.Domains.ClassCovers.Commands.UpdateCover;
using Application.Domains.ClassCovers.Queries.GetCoverWithDetails;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Identifiers;
using Core.Abstractions.Services;
using Core.Models.Covers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Threading;

[Authorize(Policy = AuthPolicies.CanEditCovers)]
public class UpdateModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public UpdateModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<UpdateModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.ShortTerm_Covers_Index;
    [ViewData] public string PageTitle { get; set; } = "New Class Cover";
    
    [BindProperty(SupportsGet = true)]
    public ClassCoverId Id { get; set; } = ClassCoverId.Empty;
    public string OfferingName { get; set; }
    [BindProperty]
    [DataType(DataType.Date)]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
    public DateOnly StartDate { get; set; }
    [BindProperty]
    [DataType(DataType.Date)]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
    public DateOnly EndDate { get; set; }
    public string TeacherSchool { get; set; }
    public string TeacherName { get; set; }

    public async Task OnGet(CancellationToken cancellationToken) => await PreparePage(cancellationToken);

    public async Task<IActionResult> OnPostUpdate(CancellationToken cancellationToken)
    {
        UpdateCoverCommand command = new(Id, StartDate, EndDate);

        _logger
            .ForContext(nameof(UpdateCoverCommand), command, true)
            .Information("Requested to update Class Cover with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result<ClassCover> result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to update Class Cover with id {Id} by user {User}", Id, _currentUserService.UserName);
            
            await PreparePage(cancellationToken);

            ModalContent = ErrorDisplay.Create(result.Error);

            return Page();
        }

        return RedirectToPage("/ShortTerm/Covers/Index", new { area = "Staff" });
    }

    private async Task PreparePage(CancellationToken cancellationToken = default)
    {
        _logger.Information("Requested to retrieve Class Cover with id {Id} for edit by user {User}", Id, _currentUserService.UserName);

        Result<CoverWithDetailsResponse> coverResult = await _mediator.Send(new GetCoverWithDetailsQuery(Id), cancellationToken);

        if (coverResult.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), coverResult.Error, true)
                .Warning("Failed to retrieve Class Cover with id {Id} for edit by user {User}", Id, _currentUserService.UserName);
            
            ModalContent = ErrorDisplay.Create(
                coverResult.Error,
                _linkGenerator.GetPathByPage("/ShortTerm/Covers/Index", values: new { area = "Staff" }));

            return;
        }

        OfferingName = coverResult.Value.OfferingName;
        StartDate = coverResult.Value.StartDate;
        EndDate = coverResult.Value.EndDate;
        TeacherName = coverResult.Value.UserName;
        TeacherSchool = coverResult.Value.UserSchool;
    }
}
