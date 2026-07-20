namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Enrolments.Applications;

using Application.Domains.EnrolmentContext.Applications.Models;
using Application.Domains.EnrolmentContext.Applications.Queries.GetEnrolmentApplicationsByPeriod;
using Application.Domains.EnrolmentContext.EnrolmentPeriods.Models;
using Application.Domains.EnrolmentContext.EnrolmentPeriods.Queries.GetAllEnrolmentPeriods;
using Application.Interfaces.Services;
using Application.Models.Auth;
using Constellation.Application.Common.PresentationModels;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Shared;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Extensions;
using Presentation.Shared.Helpers.Attributes;
using Serilog;
using Shared.Components.UploadFileForImportStaging;

[HasPermission(AuthPermission.Partners_Enrolments_Applications_View_Value)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly IImportService _importService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        IImportService importService,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _importService = importService;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForStaffPortal();
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Enrolments_Applications;
    [ViewData] public string PageTitle => "Enrolment Applications";

    [BindProperty(SupportsGet = true)]
    public EnrolmentPeriodId PeriodId { get; set; } = EnrolmentPeriodId.Empty;

    public List<EnrolmentPeriodResponse> Periods { get; set; } = [];
    public List<EnrolmentApplicationResponse> Applications { get; set; } = [];

    public async Task OnGet()
    {
        await PreparePage();

        if (PeriodId == EnrolmentPeriodId.Empty)
        {
            if (Periods.Count is 0 or > 1)
                return;

            PeriodId = Periods.First().Id;
        }

        Result<List<EnrolmentApplicationResponse>> applications = await _mediator.Send(new GetEnrolmentApplicationsByPeriodQuery(PeriodId));
        
        if (applications.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(applications.Error);

            return;
        }

        Applications = applications.Value;
    }

    private async Task PreparePage()
    {
        Result<List<EnrolmentPeriodResponse>> periods = await _mediator.Send(new GetAllEnrolmentPeriodsQuery());

        if (periods.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(periods.Error);

            return;
        }

        Periods = periods.Value
            .OrderBy(entry => entry.OpenAt)
            .ToList();
    }

    public async Task<IActionResult> OnPostImportFile(UploadFileForImportStagingSelection viewModel)
    {
        if (viewModel.UploadFile.Length == 0)
        {
            Error error = new("Page Upload", "You must select a valid file for upload");

            ModalContent = ErrorDisplay.Create(error, null);

            await PreparePage();
            return Page();
        }

        try
        {
            await using MemoryStream target = new();
            await viewModel.UploadFile.CopyToAsync(target);

            Result<Guid> key = await _importService.StageImportFile(target, viewModel.UploadFile.FileName);

            if (key.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), key.Error, true)
                    .Warning("Failed to upload file by user {User}", _currentUserService.UserName);

                ModalContent = ErrorDisplay.Create(key.Error);

                await PreparePage();
                return Page();
            }

            return RedirectToPage("/Partner/Enrolments/Applications/Import", new { area = "Staff", Key = key.Value });
        }
        catch (Exception ex)
        {
            _logger
                .ForContext(nameof(Exception), ex, true)
                .Warning("Failed to upload file by user {User}", _currentUserService.UserName);

            ModalContent = ExceptionDisplay.Create(ex);

            await PreparePage();
            return Page();
        }
    }
}