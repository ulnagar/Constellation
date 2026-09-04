namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students.Reports;

using Application.Common.PresentationModels;
using Application.Domains.Students.Models;
using Application.Domains.Students.Queries.ExportLastLoggedInDateForCurrentStudents;
using Application.Domains.Students.Queries.GetLastLoggedInDateForCurrentStudents;
using Application.Models.Auth;
using Constellation.Application.Domains.EnrolmentContext.Offers.Queries.ExportOfferList;
using Constellation.Application.Helpers;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Attributes;
using Serilog;

[HasPermission(AuthPermission.Partners_Students_View_Value)]
public class RecentLoginsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public RecentLoginsModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<RecentLoginsModel>();
    }

    [ViewData] 
    public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Reports;

    [ViewData]
    public string PageTitle => "Students with no recent login";

    public List<StudentLoginData> Logins { get; set; } = [];


    public async Task OnGet()
    {
        Result<List<StudentLoginData>> students = await _mediator.Send(new GetLastLoggedInDateForCurrentStudentsQuery());

        if (students.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(students.Error);
        }
        else
        {
            Logins = students.Value;
        }
    }

    public async Task<IActionResult> OnGetExport()
    {
        Result<byte[]> file = await _mediator.Send(new ExportLastLoggedInDateForCurrentStudentsQuery());

        if (file.IsFailure)
            return BadRequest(file.Error.Message);

        return File(file.Value, FileContentTypes.ExcelModernFile, "Student Login Times.xlsx");
    }
}