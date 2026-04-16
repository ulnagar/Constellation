namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Assessments.Provisions;

using Application.Common.PresentationModels;
using Application.Domains.Assessments.Provisions.Commands.AssignProvisionToStudent;
using Application.Domains.Assessments.Provisions.Commands.ImportStudentProvisions;
using Application.Domains.Assessments.Provisions.Commands.RemoveStudentProvision;
using Application.Domains.Assessments.Provisions.Models;
using Application.Domains.Assessments.Provisions.Queries.GetCurrentYearStudentProvisions;
using Application.Domains.Assessments.Provisions.Queries.GetStudentProvisionById;
using Application.Domains.Assessments.Provisions.Queries.GetStudentProvisions;
using Constellation.Application.Helpers;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Shared.Helpers.Attributes;
using Core.Abstractions.Services;
using Core.Errors;
using Core.Models.Assessments.Identifiers;
using Core.Models.Students.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using Shared.Components.AssignStudentProvision;
using Shared.Components.BulkImportStudentProvisions;
using Shared.PartialViews.RemoveStudentProvisionConfirmationModal;
using System.Text;

[HasPermission(AuthPermission.Subjects_AssessmentsProvisions_Assign_Value)]
public class ByStudentModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorisationService;
    private readonly ILogger _logger;

    public ByStudentModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        IAuthorizationService authorisationService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _authorisationService = authorisationService;
        _logger = logger
            .ForContext<ByStudentModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Assessments_Provisions;
    [ViewData] public string PageTitle => "Student Provisions";

    [BindProperty(SupportsGet = true)]
    public ProvisionFilter Filter { get; set; } = ProvisionFilter.Current;

    public List<StudentProvisionResponse> Provisions = [];

    public async Task OnGet()
    {
        await PreparePage();
    }

    private async Task PreparePage()
    {
        Result<List<StudentProvisionResponse>> provisions = Filter switch
        {
            ProvisionFilter.Current => await _mediator.Send(new GetCurrentYearStudentProvisionsQuery()),
            ProvisionFilter.All => await _mediator.Send(new GetStudentProvisionsQuery()),
            _ => throw new ArgumentOutOfRangeException()
        };

        if (provisions.IsFailure)
        {
            return;
        }

        Provisions = provisions.Value;
    }

    public async Task<IActionResult> OnPostAssignProvision(AssignStudentProvisionSelection viewModel)
    {
        if (viewModel.StudentId == StudentId.Empty
            || viewModel.ProvisionId == ProvisionId.Empty)
        {
            ModalContent = ErrorDisplay.Create(
                ApplicationErrors.UnknownError,
                _linkGenerator.GetPathByPage("/Subject/Assessments/Provisions/ByStudent", values: new { area = "Staff" }));

            await PreparePage();
            return Page();
        }

        Result result = await _mediator.Send(new AssignProvisionToStudentCommand(viewModel.StudentId, viewModel.ProvisionId));

        if (result.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(result.Error);

            await PreparePage();
            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAjaxRemoveProvision(StudentProvisionId studentProvisionId)
    {
        Result<StudentProvisionResponse> provision = await _mediator.Send(new GetStudentProvisionByIdQuery(studentProvisionId));

        if (provision.IsFailure)
            return BadRequest();

        RemoveStudentProvisionConfirmationModalViewModel viewModel = new(provision.Value);

        return Partial(viewModel.ViewName, viewModel);
    }

    public async Task OnGetRemoveProvision(StudentProvisionId studentProvisionId, CancellationToken cancellationToken)
    {
        AuthorizationResult authorised = await _authorisationService.AuthorizeAsync(User, AuthPermission.Subjects_AssessmentsProvisions_Assign_Value);

        if (!authorised.Succeeded)
        {
            _logger
                .ForContext(nameof(Error), DomainErrors.Permissions.Unauthorised, true)
                .Warning("Failed to remove Student Provision by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(DomainErrors.Permissions.Unauthorised);
            await PreparePage();
            return;
        }

        RemoveStudentProvisionCommand command = new(studentProvisionId);

        _logger
            .ForContext(nameof(RemoveStudentProvisionCommand), command, true)
            .Information("Requested to remove Student Provision by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to remove Student Provision by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(result.Error);
        }

        await PreparePage();
    }

    public async Task<IActionResult> OnPostImportProvisions(BulkImportStudentProvisionsSelection viewModel)
    {
        if (viewModel.UploadFile.Length == 0)
        {
            Error error = new("Page Upload", "You must select a valid file for upload");

            ModalContent = ErrorDisplay.Create(error, null);

            return Page();
        }

        try
        {
            if (viewModel.UploadFile.ContentType != FileContentTypes.ExcelModernFile)
            {
                Error error = new("Page Upload", "Only XLSX files are accepted");

                ModalContent = ErrorDisplay.Create(error, null);

                return Page();
            }

            await using MemoryStream target = new();
            await viewModel.UploadFile.CopyToAsync(target);

            Result<List<string>> result = await _mediator.Send(new ImportStudentProvisionsCommand(target));

            if (result.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), result.Error, true)
                    .Warning("Failed to upload Assessment Provisions by user {User}", _currentUserService.UserName);

                ModalContent = ErrorDisplay.Create(
                    result.Error,
                    _linkGenerator.GetPathByPage("/Subject/Assessments/Provisions/ByStudent", values: new { area = "Staff" }));

                return Page();
            }

            if (result.Value.Count > 0)
            {
                StringBuilder content = new();
                content.Append("The following errors occurred while importing the Student Provisions:");
                content.Append("<ul>");
                foreach (var item in result.Value)
                    content.Append($"<li>{item}</li>");
                content.Append("</ul>");

                ModalContent = FeedbackDisplay.Create(
                    "Import Errors",
                    content.ToString(),
                    "Ok",
                    "btn-secondary",
                    _linkGenerator.GetPathByPage("/Subject/Assessments/Provisions/ByStudent", values: new { area = "Staff" }));

                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger
                .ForContext(nameof(Exception), ex, true)
                .Warning("Failed to upload External Reports by user {User}", _currentUserService.UserName);

            ModalContent = ExceptionDisplay.Create(
                ex,
                _linkGenerator.GetPathByPage("/Subject/Assessments/Provisions/ByStudent", values: new { area = "Staff" }));

            return Page();
        }

        return RedirectToPage();
    }

    public enum ProvisionFilter
    {
        Current,
        All
    }
}