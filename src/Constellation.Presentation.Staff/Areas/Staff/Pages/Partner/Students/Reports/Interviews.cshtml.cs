namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students.Reports;

using Application.Common.PresentationModels;
using Application.DTOs;
using Application.Helpers;
using Application.Interfaces.Services;
using Application.Models.Auth;
using Application.Offerings.GetOfferingsForSelectionList;
using Application.Reports.CreateInterviewsImport;
using Constellation.Core.Models.Offerings.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.CanEditStudents)]
public class InterviewsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly IExcelService _excelService;
    private readonly LinkGenerator _linkGenerator;

    public InterviewsModel(
        ISender mediator,
        IExcelService excelService,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _excelService = excelService;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Reports;

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
        Result<List<InterviewExportDto>> data = await _mediator.Send(new CreateInterviewsImportQuery(Grades, ClassList, PerFamily, ResidentialFamilyOnly));

        if (data.IsFailure)
        {
            ModalContent = new ErrorDisplay(data.Error);

            return Page();
        }

        MemoryStream stream = await _excelService.CreatePTOFile(data.Value);

        return File(stream, FileContentTypes.ExcelModernFile, "PTO Export.xlsx");
    }
}