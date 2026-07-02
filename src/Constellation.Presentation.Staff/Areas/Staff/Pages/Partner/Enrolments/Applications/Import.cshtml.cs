namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Enrolments.Applications;

using Application.Domains.Import.Models;
using Application.Interfaces.Services;
using Application.Models.Auth;
using Application.Models.ImportCache;
using Constellation.Application.Domains.Import.Interfaces;
using Constellation.Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Attributes;
using Presentation.Shared.Helpers.Logging;
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
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Enrolments_Applications;
    [ViewData] public string PageTitle => "Import Enrolment Applications";

    [BindProperty(SupportsGet = true)]
    public Guid Key { get; set; }

    public IReadOnlyList<ImportFieldDefinition> FieldDefinitions => EnrolmentApplicationImportFields.Definitions;

    [BindProperty]
    public ColumnMappingInput Mapping { get; set; }
    
    public IReadOnlyList<string> AvailableHeaders { get; set; }

    public void OnGet()
    {
        bool success = _stagingCache.TryGet(Key, out StagedImport import);

        if (!success)
            return;

        AvailableHeaders = import.Headers;
        Mapping = new ColumnMappingInput();
    }

    public async Task<IActionResult> OnPostMap()
    {
        Mapping.Validate(FieldDefinitions, AvailableHeaders);

        return Page();
    }
}