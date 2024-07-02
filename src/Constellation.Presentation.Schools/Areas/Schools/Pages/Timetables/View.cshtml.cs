namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Timetables;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Application.Timetables.GetStudentTimetableExport;
using Constellation.Application.DTOs;
using Constellation.Application.Timetables.GetStudentTimetableData;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
public class ViewModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public ViewModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        IHttpContextAccessor httpContextAccessor, 
        IServiceScopeFactory serviceFactory) 
        : base(httpContextAccessor, serviceFactory)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Models.ActivePage.Timetables;

    [BindProperty(SupportsGet = true)]
    public string StudentId { get; set; }

    public StudentTimetableDataDto StudentTimetableData { get; set; }

    public async Task OnGet()
    {
        Result<StudentTimetableDataDto> timetableRequest = await _mediator.Send(new GetStudentTimetableDataQuery(StudentId));

        if (timetableRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(timetableRequest.Error);

            return;
        }

        StudentTimetableData = timetableRequest.Value;
    }

    public async Task<IActionResult> OnGetDownload()
    {
        Result<FileDto> request = await _mediator.Send(new GetStudentTimetableExportQuery(StudentId));

        if (request.IsFailure)
        {
            ModalContent = new ErrorDisplay(request.Error);

            return Page();
        }

        return File(request.Value.FileData, request.Value.FileType, request.Value.FileName);
    }
    
}