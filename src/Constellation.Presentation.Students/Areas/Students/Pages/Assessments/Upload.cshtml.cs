namespace Constellation.Presentation.Students.Areas.Students.Pages.Assessments;

using Application.Domains.Assessments.Assessments.Commands.AddSubmissionToAssessment;
using Application.Domains.Assessments.Assessments.Queries.GetAssessmentById;
using Application.Domains.Students.Models;
using Application.Models.Auth;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.Assessments.Assessments.Models;
using Constellation.Application.Domains.Assessments.Assessments.Queries.GetCurrentAssessmentsByStudentId;
using Constellation.Application.Domains.Students.Queries.GetStudentById;
using Constellation.Application.DTOs;
using Constellation.Application.Helpers;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Logging;
using Core.Models.Assessments.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.Attributes;
using Serilog;

[HasPermission(AuthPermission.StudentPortal_View_Value)]
[RequestSizeLimit(10485760)]
public class UploadModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public UploadModel(
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
            .ForContext(LogDefaults.Application, LogDefaults.SchoolsPortal);
    }

    [ViewData] public string ActivePage => Models.ActivePage.Assessment;

    [BindProperty(SupportsGet = true)]
    public AssessmentId AssessmentId { get; set; } = AssessmentId.Empty;
    public AssessmentResponse Assessment { get; set; }
    public StudentResponse Student { get; set; }

    [BindProperty]
    [AllowExtensions(FileExtensions: "pdf", ErrorMessage = "You can only upload PDF files")]
    public IFormFile? UploadFile { get; set; }

    public async Task OnGet() => await PreparePage();

    private async Task PreparePage()
    {
        _logger.Information("Requested to load assessments by user {user}", _currentUserService.UserName);

        string studentIdClaimValue = User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StudentId)?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(studentIdClaimValue))
        {
            _logger
                .ForContext(nameof(Error), StudentErrors.InvalidId, true)
                .Information("Requested to load assessments by user {user}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(StudentErrors.InvalidId);

            return;
        }

        StudentId studentId = StudentId.FromValue(new(studentIdClaimValue));

        Result<StudentResponse> studentRequest = await _mediator.Send(new GetStudentByIdQuery(studentId));

        if (studentRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), studentRequest.Error, true)
                .Information("Requested to load assessments by user {user}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(studentRequest.Error);

            return;
        }

        Student = studentRequest.Value;

        GetAssessmentByIdQuery query = new(AssessmentId);

        _logger
            .ForContext(nameof(GetCurrentAssessmentsByStudentIdQuery), query, true)
            .Information("Requested to load assessments by user {user}", _currentUserService.UserName);

        Result<AssessmentResponse> assessment = await _mediator.Send(query);

        if (assessment.IsFailure)
        {
            _logger
                .ForContext(nameof(GetCurrentAssessmentsByStudentIdQuery), query, true)
                .ForContext(nameof(Error), assessment.Error, true)
                .Warning("Failed to load assessments by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(assessment.Error);

            return;
        }
        
        Assessment = assessment.Value;
    }

    public async Task<IActionResult> OnPostSubmit()
    {
        string studentIdClaimValue = User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StudentId)?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(studentIdClaimValue))
        {
            _logger
                .ForContext(nameof(Error), StudentErrors.InvalidId, true)
                .Information("Requested to load assessments by user {user}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(StudentErrors.InvalidId);

            return Page();
        }

        StudentId studentId = StudentId.FromValue(new(studentIdClaimValue));

        if (UploadFile is null)
        {
            ModalContent = ErrorDisplay.Create(
                new("FileEmpty", "You must select a file to upload"),
                _linkGenerator.GetPathByPage("/Assessments/Upload", values: new { area = "Students", AssessmentId }));

            return Page();
        }

        if (UploadFile.ContentType != FileContentTypes.PdfFile)
        {
            ModalContent = ErrorDisplay.Create(
                new("FileTypeMismatch", "You can only upload PDF files"),
                _linkGenerator.GetPathByPage("/Assessments/Upload", values: new { area = "Students", AssessmentId }));

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
            studentId,
            file);

        _logger.Information("Requested to upload assessment submission by user {user} with file {file}", _currentUserService.UserName, file.FileName);

        Result request = await _mediator.Send(command);

        if (request.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                request.Error,
                _linkGenerator.GetPathByPage("/Assessments/Upload", values: new { area = "Students", AssessmentId }));

            return Page();
        }

        ModalContent = FeedbackDisplay.Create(
            "Upload Successful",
            "The file has been uploaded successfully. You will receive an email receipt shortly.",
            "Ok",
            "btn-success",
            _linkGenerator.GetPathByPage("/Assessments/Index", values: new { area = "Students" }));

        await PreparePage();

        return Page();
    }
}