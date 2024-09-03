namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students;

using Application.Common.PresentationModels;
using Constellation.Application.Enrolments.EnrolStudent;
using Constellation.Application.Enrolments.GetStudentEnrolmentsWithDetails;
using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.GetOfferingsForBulkEnrol;
using Constellation.Application.Students.GetStudentById;
using Constellation.Application.Students.Models;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using Core.Abstractions.Services;
using Core.Models.Students.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;

[Authorize(Policy = AuthPolicies.CanEditStudents)]
public class BulkEnrolModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public BulkEnrolModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<BulkEnrolModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Students;
    [ViewData] public string PageTitle { get; set; } = "Bulk Enrol";

    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(ConstructorBinder))]
    public StudentId Id { get; set; }

    public StudentResponse Student { get; set; }
    public List<BulkEnrolOfferingResponse> Offerings { get; set; } = new();
    public List<BulkEnrolOfferingResponse> ExistingEnrolments { get; set; } = new();

    [BindProperty]
    public List<Guid> SelectedOfferingIds { get; set; } = new();

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve bulk enrol data for Student with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result<StudentResponse> studentRequest = await _mediator.Send(new GetStudentByIdQuery(Id));

        if (studentRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), studentRequest.Error, true)
                .Warning("Failed to retrieve bulk enrol data for Student with id {Id} by user {User}", Id, _currentUserService.UserName);

            return;
        }

        Student = studentRequest.Value;

        PageTitle = $"Enrol {Student.Name.DisplayName}";

        Result<List<BulkEnrolOfferingResponse>> offeringRequest = await _mediator.Send(new GetOfferingsForBulkEnrolQuery(Student.Grade));

        if (offeringRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), offeringRequest.Error, true)
                .Warning("Failed to retrieve bulk enrol data for Student with id {Id} by user {User}", Id, _currentUserService.UserName);

            return;
        }

        Offerings = offeringRequest.Value;

        Result<List<StudentEnrolmentResponse>> enrolmentRequest = await _mediator.Send(new GetStudentEnrolmentsWithDetailsQuery(Id));

        if (enrolmentRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), enrolmentRequest.Error, true)
                .Warning("Failed to retrieve bulk enrol data for Student with id {Id} by user {User}", Id, _currentUserService.UserName);

            return;
        }

        foreach (StudentEnrolmentResponse enrolment in enrolmentRequest.Value)
        {
            BulkEnrolOfferingResponse? offeringEntry = Offerings.FirstOrDefault(entry => entry.OfferingId == enrolment.OfferingId);

            if (offeringEntry is not null)
            {
                ExistingEnrolments.Add(offeringEntry);
            }
        }
    }

    public async Task<IActionResult> OnPost()
    {
        foreach (Guid offeringGuid in SelectedOfferingIds)
        {
            OfferingId offeringId = OfferingId.FromValue(offeringGuid);

            EnrolStudentCommand command = new(Id, offeringId);

            _logger
                .ForContext(nameof(EnrolStudentCommand), command, true)
                .Information("Requested to enrol Student into Offering by user {User}", _currentUserService.UserName);

            Result response = await _mediator.Send(command);

            if (response.IsFailure)
            {
                ModalContent = new ErrorDisplay(
                    response.Error,
                    _linkGenerator.GetPathByPage("/Partner/Students/Details", values: new { area = "Staff", id = Id }));

                _logger
                    .ForContext(nameof(Error), response.Error, true)
                    .Warning("Failed to enrol Student into Offering by user {User}", _currentUserService.UserName);

                return Page();
            }
        }

        return RedirectToPage("/Partner/Students/Details", new { area = "Staff", Id = Id });
    }
}