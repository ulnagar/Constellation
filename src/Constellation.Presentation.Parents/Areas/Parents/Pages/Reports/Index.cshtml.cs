namespace Constellation.Presentation.Parents.Areas.Parents.Pages.Reports;

using Application.Attachments.GetAttachmentFile;
using Application.Models.Auth;
using Application.Students.GetStudentsByParentEmail;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Reports.GetAcademicReportList;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.ValueObjects;
using Core.Models.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;

[Authorize(Policy = AuthPolicies.IsParent)]
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
            .ForContext("APPLICATION", "Parent Portal");
    }

    [ViewData] public string ActivePage => Models.ActivePage.Reports;

    [BindProperty(SupportsGet = true)]
    public string StudentId { get; set; } = string.Empty;

    public StudentResponse? SelectedStudent { get; set; }

    public List<StudentResponse> Students { get; set; } = new();

    public List<AcademicReportResponse> Reports { get; set; } = new();

    public async Task OnGet() => await PreparePage();

    public async Task<IActionResult> OnGetDownload(
        [ModelBinder(typeof(ConstructorBinder))] AcademicReportId reportId)
    {
        Result<AttachmentResponse> fileResponse = await _mediator.Send(new GetAttachmentFileQuery(AttachmentType.StudentReport, reportId.ToString()));

        if (fileResponse.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                fileResponse.Error,
                _linkGenerator.GetPathByPage("/Reports/Index", values: new { area = "Parents" }));

            await PreparePage();

            return Page();
        }

        return File(fileResponse.Value.FileData, fileResponse.Value.FileType, fileResponse.Value.FileName);
    }

    public async Task PreparePage()
    {
        _logger.Information("Requested to retrieve student list by user {user}", _currentUserService.UserName);

        Result<List<StudentResponse>> studentsRequest = await _mediator.Send(new GetStudentsByParentEmailQuery(_currentUserService.EmailAddress));

        if (studentsRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(studentsRequest.Error);

            return;
        }

        Students = studentsRequest.Value
            .OrderBy(student => student.CurrentGrade)
            .ThenBy(student => student.LastName)
            .ThenBy(student => student.FirstName)
            .ToList();

        if (Students.Count == 1)
            StudentId = Students.First().StudentId;

        if (!string.IsNullOrWhiteSpace(StudentId))
        {
            _logger.Information("Requested to retrieve reports by user {user} for student {student}", _currentUserService.UserName, StudentId);

            Result<List<AcademicReportResponse>> reportsRequest = await _mediator.Send(new GetAcademicReportListQuery(StudentId));

            if (reportsRequest.IsFailure)
            {
                ModalContent = new ErrorDisplay(reportsRequest.Error);

                return;
            }

            Reports = reportsRequest.Value;
            SelectedStudent = Students.FirstOrDefault(entry => entry.StudentId == StudentId);
        }
    }
}