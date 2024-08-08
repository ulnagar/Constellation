namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students.Reports;

using Application.Common.PresentationModels;
using Application.DTOs;
using Application.Helpers;
using Application.Interfaces.Services;
using Application.Models.Auth;
using Application.Offerings.GetOfferingsForSelectionList;
using Application.Reports.CreateInterviewsImport;
using Constellation.Core.Models.Offerings.Identifiers;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Models;
using Serilog;

[Authorize(Policy = AuthPolicies.CanEditStudents)]
public class InterviewsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly IExcelService _excelService;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public InterviewsModel(
        ISender mediator,
        IExcelService excelService,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _excelService = excelService;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<InterviewsModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Reports;
    [ViewData] public string PageTitle => "PTO Setup";

    [BindProperty]
    public List<int> Grades { get; set; }
    
    [BindProperty]
    public List<OfferingId> ClassList { get; set; }
    
    [BindProperty]
    public bool PerFamily { get; set; }
    
    [BindProperty]
    public bool ResidentialFamilyOnly { get; set; }

    public SelectList AllClasses { get; set; }

    public async Task OnGet()
    {
        Result<List<OfferingSelectionListResponse>> offerings = await _mediator.Send(new GetOfferingsForSelectionListQuery());

        if (offerings.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                offerings.Error,
                _linkGenerator.GetPathByPage("/Partner/Students/Reports/Index", values: new { area = "Staff" }));

            return;
        }

        AllClasses = new SelectList(offerings.Value, "Id", "Name");
    }

    public async Task<IActionResult> OnPost()
    {
        CreateInterviewsImportQuery command = new(Grades, ClassList, PerFamily, ResidentialFamilyOnly);

        _logger
            .ForContext(nameof(CreateInterviewsImportQuery), command, true)
            .Information("Requested to build PTO Setup file by user {User}", _currentUserService.UserName);

        Result<List<InterviewExportDto>> data = await _mediator.Send(command);

        if (data.IsFailure)
        {
            ModalContent = new ErrorDisplay(data.Error);

            _logger
                .ForContext(nameof(Error), data, true)
                .Warning("Failed to build PTO Setup file by user {User}", _currentUserService.UserName);

            return Page();
        }

        MemoryStream stream = await _excelService.CreatePTOFile(data.Value);

        return File(stream, FileContentTypes.ExcelModernFile, "PTO Export.xlsx");
    }
}