namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Enrolments.Applications;

using Application.Domains.EnrolmentContext.Applications.Commands.ImportApplications;
using Application.Domains.Import.Models;
using Application.Interfaces.Services;
using Application.Models.Auth;
using Application.Models.ImportCache;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Models;
using Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Queries.GetAllEnrolmentPeriods;
using Constellation.Application.Domains.Import.Interfaces;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using Constellation.Core.Shared;
using Core.Models.EnrolmentContext.Application;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Extensions;
using Presentation.Shared.Helpers.Attributes;
using Serilog;

[HasPermission(AuthPermission.Partners_Enrolments_Applications_Edit_Value)]
public class ImportModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly IImportStagingCache _stagingCache;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public ImportModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        IImportStagingCache stagingCache,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _stagingCache = stagingCache;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<ImportModel>()
            .ForStaffPortal();
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Enrolments_Applications;
    [ViewData] public string PageTitle => "Import Enrolment Applications";

    [BindProperty(SupportsGet = true)]
    public Guid Key { get; set; }

    [BindProperty]
    public EnrolmentPeriodId PeriodId { get; set; } = EnrolmentPeriodId.Empty;

    public List<EnrolmentPeriodResponse> Periods { get; set; } = [];

    public IReadOnlyList<ImportFieldDefinition> FieldDefinitions => EnrolmentApplicationImportFields.Definitions;

    [BindProperty]
    public ColumnMapping Mapping { get; set; }
    
    public IReadOnlyList<string> AvailableHeaders { get; set; }

    public bool ImportFinished { get; set; }
    public ImportRunResult<Application>? ImportResult { get; set; }

    public async Task OnGet()
    {
        await PreparePage();

        Mapping = new ColumnMapping();
    }

    private async Task PreparePage()
    {
        bool success = _stagingCache.TryGet(Key, out StagedImport import);

        if (!success)
            return;

        AvailableHeaders = import.Headers;

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

    public async Task<IActionResult> OnPostMap()
    {
        await PreparePage();

        Result validation = Mapping.Validate(FieldDefinitions, AvailableHeaders, true);

        if (validation.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(validation.Error);

            return Page();
        }

        Result<ImportRunResult<Application>> import = await _mediator.Send(new ImportApplicationsCommand(PeriodId, Mapping));

        ImportFinished = true;

        if (import.IsSuccess)
            ImportResult = import.Value;
        else
            ModalContent = ErrorDisplay.Create(import.Error);

        return Page();
    }
}