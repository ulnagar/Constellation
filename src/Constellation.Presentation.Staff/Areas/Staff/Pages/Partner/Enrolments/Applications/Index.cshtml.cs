namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Enrolments.Applications;

using Application.Domains.EnrolmentContext.Applications.Models;
using Application.Domains.EnrolmentContext.Applications.Queries.ExportApplicationsList;
using Application.Domains.EnrolmentContext.Applications.Queries.GetEnrolmentApplicationsByPeriod;
using Application.Domains.EnrolmentContext.EnrolmentPeriods.Models;
using Application.Domains.EnrolmentContext.EnrolmentPeriods.Queries.GetAllEnrolmentPeriods;
using Application.Helpers;
using Application.Interfaces.Services;
using Application.Models.Auth;
using Constellation.Application.Common.PresentationModels;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Shared;
using Core.Models.EnrolmentContext.Application.Enums;
using Core.Models.EnrolmentContext.Application.Identifiers;
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

    [BindProperty(SupportsGet = true)]
    public StatusFilter Status { get; set; } = StatusFilter.All;

    public List<EnrolmentPeriodResponse> Periods { get; set; } = [];
    public List<EnrolmentApplicationResponse> Applications { get; set; } = [];

    public async Task<IActionResult> OnGet() => await PreparePage();

    private async Task<IActionResult> PreparePage()
    {
        Result<List<EnrolmentPeriodResponse>> periods = await _mediator.Send(new GetAllEnrolmentPeriodsQuery());

        if (periods.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(periods.Error);

            return Page();
        }

        Periods = periods.Value
            .OrderBy(entry => entry.OpenAt)
            .ToList();

        if (PeriodId == EnrolmentPeriodId.Empty)
        {
            if (Periods.Count is 0 or > 1)
                return Page();

            return RedirectToPage(new { PeriodId = Periods.First().Id });
        }

        Result<List<EnrolmentApplicationResponse>> applications = await _mediator.Send(new GetEnrolmentApplicationsByPeriodQuery(PeriodId));

        if (applications.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(applications.Error);

            return Page();
        }

        Applications = FilterApplications(applications.Value, Status);

        return Page();
    }

    public async Task<IActionResult> OnPostImportFile(UploadFileForImportStagingSelection viewModel)
    {
        if (viewModel.UploadFile.Length == 0)
        {
            Error error = new("Page Upload", "You must select a valid file for upload");

            ModalContent = ErrorDisplay.Create(error, null);

            return await PreparePage();
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

                return await PreparePage();
            }

            return RedirectToPage("/Partner/Enrolments/Applications/Import", new { area = "Staff", Key = key.Value });
        }
        catch (Exception ex)
        {
            _logger
                .ForContext(nameof(Exception), ex, true)
                .Warning("Failed to upload file by user {User}", _currentUserService.UserName);

            ModalContent = ExceptionDisplay.Create(ex);

            return await PreparePage();
        }
    }

    public async Task<IActionResult> OnPostExport(List<ApplicationId> applicationIds)
    {
        Result<byte[]> file = await _mediator.Send(new ExportApplicationsListQuery(applicationIds));

        if (file.IsFailure)
            return BadRequest(file.Error.Message);
        
        return File(file.Value, FileContentTypes.ExcelModernFile, "Enrolment Application Export.xlsx");
    }

    public enum StatusFilter
    {
        All,
        Pending,
        Approved,
        Rejected
    }

    private static List<EnrolmentApplicationResponse> FilterApplications(
        IEnumerable<EnrolmentApplicationResponse> applications,
        StatusFilter filter)
    {
        return filter switch
        {
            StatusFilter.All => applications.ToList(),

            StatusFilter.Pending => applications
                .Where(application => application.Status == ApplicationStatus.Pending)
                .ToList(),

            StatusFilter.Approved => applications
                .Where(application => application.Status == ApplicationStatus.Approved)
                .ToList(),

            StatusFilter.Rejected => applications
                .Where(application => application.Status == ApplicationStatus.Rejected)
                .ToList(),

            _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, null)
        };
    }
}