namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students;

using Constellation.Application.Absences.GetAbsenceSummaryForStudent;
using Constellation.Application.Assets.GetDevicesAllocatedToStudent;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Enrolments.GetStudentEnrolmentsWithDetails;
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
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Threading;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DetailsModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly IAuthorizationService _authorizationService;

    public DetailsModel(
        IMediator mediator,
        LinkGenerator linkGenerator,
        IAuthorizationService authorizationService)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _authorizationService = authorizationService;
    }

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Students;

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; }

    public StudentResponse Student { get; set; }

    public List<FamilyContactResponse> FamilyContacts { get; set; } = new();

    public List<StudentEnrolmentResponse> Enrolments { get; set; } = new();

    public List<StudentSessionDetailsResponse> Sessions { get; set; } = new();
    public int MinPerFn => CalculateTotalSessionDuration();

    public List<StudentDeviceResponse> Equipment { get; set; } = new();

    public List<StudentAbsenceSummaryResponse> Absences { get; set; } = new();

    public RecordLifecycleDetailsResponse RecordLifecycle { get; set; }

    public async Task OnGet(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            GenerateError(new("Page.Parameter.NotFound", "You must specify a value for the Student Id parameter"));
            return;
        }

        await PreparePage(cancellationToken);
    }

    private async Task<bool> PreparePage(CancellationToken cancellationToken)
    {
        var studentRequest = await _mediator.Send(new GetStudentByIdQuery(Id), cancellationToken);

        if (studentRequest.IsFailure)
        {
            GenerateError(studentRequest.Error);
            return false;
        }

        Student = studentRequest.Value;

        var familyRequest = await _mediator.Send(new GetFamilyContactsForStudentQuery(Id), cancellationToken);

        FamilyContacts = familyRequest.IsSuccess ? familyRequest.Value : new();

        var enrolmentRequest = await _mediator.Send(new GetStudentEnrolmentsWithDetailsQuery(Id), cancellationToken);

        Enrolments = enrolmentRequest.IsSuccess ? enrolmentRequest.Value : new();

        var sessionRequest = await _mediator.Send(new GetSessionDetailsForStudentQuery(Id), cancellationToken);

        Sessions = sessionRequest.IsSuccess ? sessionRequest.Value : new();

        var equipmentRequest = await _mediator.Send(new GetDevicesAllocatedToStudentQuery(Id), cancellationToken);

        Equipment = equipmentRequest.IsSuccess ? equipmentRequest.Value : new();

        var absencesRequest = await _mediator.Send(new GetAbsenceSummaryForStudentQuery(Id), cancellationToken);

        Absences = absencesRequest.IsSuccess ? absencesRequest.Value : new();

        var lifecycleRequest = await _mediator.Send(new GetLifecycleDetailsForStudentQuery(Id), cancellationToken);

        RecordLifecycle = lifecycleRequest.IsSuccess ? lifecycleRequest.Value : new(string.Empty, DateTime.MinValue, string.Empty, DateTime.MinValue, string.Empty, DateTime.MinValue);
        
        return true;
    }

    public async Task OnGetWithdraw(CancellationToken cancellationToken)
    {
        var authorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditStudents);

        if (authorised.Succeeded)
        {
            await _mediator.Send(new WithdrawStudentCommand(Id), cancellationToken);
        }
        else
        {
            GenerateError(DomainErrors.Permissions.Unauthorised);
        }

        await PreparePage(cancellationToken);
    }

    public async Task OnGetReinstate(CancellationToken cancellationToken)
    {
        var authorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditStudents);

        if (authorised.Succeeded)
        {
            await _mediator.Send(new ReinstateStudentCommand(Id), cancellationToken);
        }
        else
        {
            GenerateError(DomainErrors.Permissions.Unauthorised);
        }

        await PreparePage(cancellationToken);
    }

    private void GenerateError(Error error)
    {
        Error = new ErrorDisplay
        {
            Error = error,
            RedirectPath = _linkGenerator.GetPathByAction("Index", "Students", new { area = "Partner" })
        };

        Student = new("", "", Core.Enums.Grade.SpecialProgram, "", "", "", false);
    }

    private int CalculateTotalSessionDuration()
    {
        if (Sessions is null || !Sessions.Any())
            return 0;

        return Sessions.Sum(session => session.Duration);
    }
}