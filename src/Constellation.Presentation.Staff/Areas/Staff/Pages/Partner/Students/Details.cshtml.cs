namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students;

using Application.Common.PresentationModels;
using Application.OfferingEnrolments.GetStudentEnrolmentsWithDetails;
using Application.OfferingEnrolments.UnenrolStudent;
using Application.OfferingEnrolments.UnenrolStudentFromAllOfferings;
using Application.Students.GetSchoolEnrolmentHistoryForStudent;
using Application.Students.RemoveSchoolEnrolment;
using Application.Students.TransferStudent;
using Constellation.Application.Absences.GetAbsenceSummaryForStudent;
using Constellation.Application.Assets.GetDevicesAllocatedToStudent;
using Constellation.Application.Families.GetFamilyContactsForStudent;
using Constellation.Application.Families.Models;
using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.GetSessionDetailsForStudent;
using Constellation.Application.Students.GetLifecycleDetailsForStudent;
using Constellation.Application.Students.GetStudentById;
using Constellation.Application.Students.Models;
using Constellation.Application.Students.ReinstateStudent;
using Constellation.Application.Students.WithdrawStudent;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using Core.Abstractions.Services;
using Core.Models.Offerings.Identifiers;
using Core.Models.Students.Enums;
using Core.Models.Students.Identifiers;
using Core.Models.Students.ValueObjects;
using Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using Shared.Components.ReinstateStudent;
using Shared.Components.TransferStudent;
using System.Threading;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public DetailsModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        IAuthorizationService authorizationService,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<DetailsModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Students;
    [ViewData] public string PageTitle { get; set; } = "Student Details";

    [BindProperty(SupportsGet = true)]
    public StudentId Id { get; set; } = StudentId.Empty;

    public StudentResponse Student { get; set; }

    public List<FamilyContactResponse> FamilyContacts { get; set; } = new();

    public List<StudentEnrolmentResponse> Enrolments { get; set; } = new();

    public List<StudentSessionDetailsResponse> Sessions { get; set; } = new();
    public int MinPerFn => CalculateTotalSessionDuration();

    public List<StudentDeviceResponse> Equipment { get; set; } = new();

    public List<StudentAbsenceSummaryResponse> Absences { get; set; } = new();

    public List<SchoolEnrolmentResponse> SchoolEnrolments { get; set; } = new();

    public RecordLifecycleDetailsResponse RecordLifecycle { get; set; } = new(string.Empty, DateTime.MinValue, string.Empty, DateTime.MinValue, string.Empty, DateTime.MinValue);

    public async Task OnGet(CancellationToken cancellationToken)
    {
        if (Id == StudentId.Empty)
        {
            GenerateError(new("Page.Parameter.NotFound", "You must specify a value for the Student Id parameter"));
            return;
        }

        await PreparePage(cancellationToken);
    }

    private async Task<bool> PreparePage(CancellationToken cancellationToken)
    {
        _logger.Information("Requested to retrieve details of Student with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result<StudentResponse> studentRequest = await _mediator.Send(new GetStudentByIdQuery(Id), cancellationToken);

        if (studentRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), studentRequest.Error, true)
                .Warning("Failed to retrieve details of Student with id {Id} by user {User}", Id, _currentUserService.UserName);

            GenerateError(studentRequest.Error);
            return false;
        }

        Student = studentRequest.Value;

        PageTitle = $"Details - {Student.Name.DisplayName}";

        Result<List<SchoolEnrolmentResponse>> schoolEnrolmentRequest = await _mediator.Send(new GetSchoolEnrolmentHistoryForStudentQuery(Id), cancellationToken);

        SchoolEnrolments = schoolEnrolmentRequest.IsSuccess ? schoolEnrolmentRequest.Value : new();

        Result<List<FamilyContactResponse>> familyRequest = await _mediator.Send(new GetFamilyContactsForStudentQuery(Id), cancellationToken);

        FamilyContacts = familyRequest.IsSuccess ? familyRequest.Value : new();

        Result<List<StudentEnrolmentResponse>> enrolmentRequest = await _mediator.Send(new GetStudentEnrolmentsWithDetailsQuery(Id), cancellationToken);

        Enrolments = enrolmentRequest.IsSuccess ? enrolmentRequest.Value : new();

        Result<List<StudentSessionDetailsResponse>> sessionRequest = await _mediator.Send(new GetSessionDetailsForStudentQuery(Id), cancellationToken);

        Sessions = sessionRequest.IsSuccess ? sessionRequest.Value : new();

        Result<List<StudentDeviceResponse>> equipmentRequest = await _mediator.Send(new GetDevicesAllocatedToStudentQuery(Id), cancellationToken);

        Equipment = equipmentRequest.IsSuccess ? equipmentRequest.Value : new();

        Result<List<StudentAbsenceSummaryResponse>> absencesRequest = await _mediator.Send(new GetAbsenceSummaryForStudentQuery(Id), cancellationToken);

        Absences = absencesRequest.IsSuccess ? absencesRequest.Value : new();

        Result<RecordLifecycleDetailsResponse> lifecycleRequest = await _mediator.Send(new GetLifecycleDetailsForStudentQuery(Id), cancellationToken);

        RecordLifecycle = lifecycleRequest.IsSuccess ? lifecycleRequest.Value : new(string.Empty, DateTime.MinValue, string.Empty, DateTime.MinValue, string.Empty, DateTime.MinValue);

        return true;
    }

    public async Task OnGetWithdraw(CancellationToken cancellationToken)
    {
        AuthorizationResult authorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditStudents);

        if (!authorised.Succeeded)
        {
            _logger
                .ForContext(nameof(Error), DomainErrors.Permissions.Unauthorised, true)
                .Warning("Failed to withdraw Student by user {User}", _currentUserService.UserName);

            GenerateError(DomainErrors.Permissions.Unauthorised);
            await PreparePage(cancellationToken);
            return;
        }

        WithdrawStudentCommand command = new(Id);

        _logger
            .ForContext(nameof(WithdrawStudentCommand), command, true)
            .Information("Requested to withdraw Student by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to withdraw Student by user {User}", _currentUserService.UserName);

            GenerateError(result.Error);
        }

        await PreparePage(cancellationToken);
    }

    public async Task<IActionResult> OnPostReinstate(
        ReinstateStudentSelection viewModel,
        CancellationToken cancellationToken)
    {
        AuthorizationResult authorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditStudents);

        if (!authorised.Succeeded)
        {
            _logger
                .ForContext(nameof(Error), DomainErrors.Permissions.Unauthorised, true)
                .Warning("Failed to reinstate Student by user {User}", _currentUserService.UserName);

            GenerateError(DomainErrors.Permissions.Unauthorised);
            await PreparePage(cancellationToken);
            return Page();
        }
        
        ReinstateStudentCommand command = new(Id, viewModel.SchoolCode, viewModel.Grade);

        _logger
            .ForContext(nameof(ReinstateStudentCommand), command, true)
            .Information("Requested to reinstate Student by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to reinstate Student by user {User}", _currentUserService.UserName);

            GenerateError(result.Error);
            await PreparePage(cancellationToken);
            return Page();
        }

        return RedirectToPage();
    }

    public async Task OnGetUnenrol(
        OfferingId offeringId, 
        CancellationToken cancellationToken)
    {
        AuthorizationResult authorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditStudents);

        if (!authorised.Succeeded)
        {
            _logger
                .ForContext(nameof(Error), DomainErrors.Permissions.Unauthorised, true)
                .Warning("Failed to unenrol Student from Offering by user {User}", _currentUserService.UserName);

            GenerateError(DomainErrors.Permissions.Unauthorised);
            await PreparePage(cancellationToken);
            return;
        }

        UnenrolStudentCommand command = new(Id, offeringId);

        _logger
            .ForContext(nameof(UnenrolStudentCommand), command, true)
            .Information("Requested to unenrol Student from Offering by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to unenrol Student from Offering by user {User}", _currentUserService.UserName);

            GenerateError(result.Error);
        }

        await PreparePage(cancellationToken);
    }

    public async Task OnGetBulkUnenrol(CancellationToken cancellationToken)
    {
        AuthorizationResult authorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditStudents);

        if (!authorised.Succeeded)
        {
            _logger
                .ForContext(nameof(Error), DomainErrors.Permissions.Unauthorised, true)
                .Warning("Failed to bulk unenrol Student from Offerings by user {User}", _currentUserService.UserName);

            GenerateError(DomainErrors.Permissions.Unauthorised);
            await PreparePage(cancellationToken);
            return;
        }

        UnenrolStudentFromAllOfferingsCommand command = new(Id);

        _logger
            .ForContext(nameof(UnenrolStudentCommand), command, true)
            .Information("Requested to bulk unenrol Student from Offerings by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to bulk unenrol Student from Offerings by user {User}", _currentUserService.UserName);

            GenerateError(result.Error);
        }

        await PreparePage(cancellationToken);
    }

    public async Task<IActionResult> OnPostTransferStudent(
        TransferStudentSelection viewModel,
        CancellationToken cancellationToken)
    {
        AuthorizationResult authorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditStudents);

        if (!authorised.Succeeded)
        {
            _logger
                .ForContext(nameof(Error), DomainErrors.Permissions.Unauthorised, true)
                .Warning("Failed to transfer Student to new School/Grade by user {User}", _currentUserService.UserName);

            GenerateError(DomainErrors.Permissions.Unauthorised);
            await PreparePage(cancellationToken);
            return Page();
        }

        TransferStudentCommand command = new(
            Id,
            viewModel.SchoolCode,
            viewModel.Grade,
            viewModel.StartDate);

        _logger
            .ForContext(nameof(TransferStudentCommand), command, true)
            .Information("Requested to transfer Student to new School/Grade by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to transfer Student to new School/Grade by user {User}", _currentUserService.UserName);

            GenerateError(result.Error);
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetRemoveSchoolEnrolment(
        SchoolEnrolmentId enrolmentId,
        CancellationToken cancellationToken = default)
    {
        AuthorizationResult authorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditStudents);

        if (!authorised.Succeeded)
        {
            _logger
                .ForContext(nameof(Error), DomainErrors.Permissions.Unauthorised, true)
                .Warning("Failed to remove School Enrolment by user {User}", _currentUserService.UserName);

            GenerateError(DomainErrors.Permissions.Unauthorised);
            await PreparePage(cancellationToken);
            return Page();
        }

        RemoveSchoolEnrolmentCommand command = new(
            Id,
            enrolmentId);

        _logger
            .ForContext(nameof(RemoveSchoolEnrolmentCommand), command, true)
            .Information("Requested to remove School Enrolment by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to remove School Enrolment by user {User}", _currentUserService.UserName);

            GenerateError(result.Error);
        }

        return RedirectToPage();
    }

    private void GenerateError(Error error)
    {
        ModalContent = new ErrorDisplay(
            error,
            _linkGenerator.GetPathByPage("/Partner/Students/Index", values: new { area = "Staff" }));
        
        Student = new(StudentId.Empty, StudentReferenceNumber.Empty, Name.Create("John", "", "Doe").Value, Gender.NonBinary, Core.Enums.Grade.SpecialProgram, EmailAddress.None, "", "", false, false);
    }

    private int CalculateTotalSessionDuration()
    {
        if (!Sessions.Any())
            return 0;

        return Sessions.Sum(session => session.Duration);
    }
}