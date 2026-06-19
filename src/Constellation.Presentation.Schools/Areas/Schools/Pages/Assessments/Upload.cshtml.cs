namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Assessments;

using Application.Domains.Assessments.Assessments.Commands.AddSubmissionToAssessment;
using Application.Domains.Assessments.Assessments.Queries.GetAssessmentByIdAndSchoolCode;
using Application.Models.Auth;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.Assessments.Assessments.Models;
using Constellation.Application.DTOs;
using Constellation.Application.Helpers;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Logging;
using Core.Models.Assessments.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Attributes;
using Serilog;

[HasPermission(AuthPermission.SchoolsPortal_Assessments_Submit_Value)]
[RequestSizeLimit(10485760)]
public class UploadModel : BasePageModel
{
    private ISender _mediator => Mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public UploadModel(
        LinkGenerator linkGenerator, 
        ICurrentUserService currentUserService, 
        ILogger logger)
    {
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.SchoolsPortal);
    }

    [ViewData] public string ActivePage => Models.ActivePage.Assessments;

    [BindProperty(SupportsGet = true)]
    public AssessmentId AssessmentId { get; set; } = AssessmentId.Empty;

    public AssessmentDetailsResponse Assessment { get; set; }

    public SelectList Students { get; set; }
    [BindProperty]
    public StudentId StudentId { get; set; }

    [BindProperty]
    [AllowExtensions(FileExtensions: "pdf", ErrorMessage = "You can only upload PDF files")]
    public IFormFile? UploadFile { get; set; }

    public async Task OnGet() => await PreparePage();

    private async Task PreparePage()
    {
        GetAssessmentByIdAndSchoolCodeQuery query = new(AssessmentId, CurrentSchoolCode);

        _logger
            .ForContext(nameof(GetAssessmentByIdAndSchoolCodeQuery), query, true)
            .Information("Requested to load assessment by user {user}", _currentUserService.UserName);

        Result<AssessmentDetailsResponse> assessment = await _mediator.Send(query);

        if (assessment.IsFailure)
        {
            _logger
                .ForContext(nameof(GetAssessmentByIdAndSchoolCodeQuery), query, true)
                .ForContext(nameof(Error), assessment.Error, true)
                .Warning("Failed to load assessment by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(assessment.Error);

            return;
        }

        Assessment = assessment.Value;
        Students = new SelectList(
            Assessment.Students,
            nameof(AssessmentDetailsResponse.Student.StudentId),
            nameof(AssessmentDetailsResponse.Student.StudentName));
    }

    public async Task<IActionResult> OnPostSubmit()
    {
        if (UploadFile is null)
        {
            ModalContent = ErrorDisplay.Create(
                new("FileEmpty", "You must select a file to upload"),
                _linkGenerator.GetPathByPage("/Assessments/Upload", values: new { area = "Schools", AssessmentId }));

            return Page();
        }

        if (UploadFile.ContentType != FileContentTypes.PdfFile)
        {
            ModalContent = ErrorDisplay.Create(
                new("FileTypeMismatch", "You can only upload PDF files"),
                _linkGenerator.GetPathByPage("/Assessments/Upload", values: new { area = "Schools", AssessmentId }));

            return Page();
        }

        await using MemoryStream target = new();
        await UploadFile.CopyToAsync(target);

        FileDto file = new()
        {
            FileData = target.ToArray(),
            FileName = UploadFile.FileName,
            FileType = UploadFile.ContentType
        };

        AddSubmissionToAssessmentCommand command = new(
            AssessmentId,
            StudentId,
            file);

        _logger.Information("Requested to upload assessment submission by user {user} with file {file}", _currentUserService.UserName, file.FileName);

        Result request = await _mediator.Send(command);

        if (request.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                request.Error,
                _linkGenerator.GetPathByPage("/Assessments/Upload", values: new { area = "Schools", AssessmentId }));

            return Page();
        }

        ModalContent = FeedbackDisplay.Create(
            "Upload Successful",
            "The file has been uploaded successfully. You will receive an email receipt shortly.",
            "Ok",
            "btn-success",
            _linkGenerator.GetPathByPage("/Assessments/Index", values: new { area = "Schools" }));

        await PreparePage();

        return Page();
    }
}