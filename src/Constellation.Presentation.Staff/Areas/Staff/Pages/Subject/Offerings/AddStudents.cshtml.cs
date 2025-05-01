namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Offerings;

using Application.Common.PresentationModels;
using Application.Domains.Offerings.Queries.GetOfferingSummary;
using Application.Domains.Students.Queries.GetCurrentStudentsFromGrade;
using Constellation.Application.Domains.Enrolments.Commands.EnrolMultipleStudentsInOffering;
using Constellation.Application.Domains.Enrolments.Queries.GetCurrentEnrolmentsForOffering;
using Constellation.Application.Domains.Offerings.Models;
using Constellation.Application.Domains.Students.Models;
using Constellation.Application.Domains.Students.Queries.GetStudentsFromOfferingGrade;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using Core.Abstractions.Services;
using Core.Enums;
using Core.Models.Students.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.CanEditSubjects)]
public class AddStudentsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public AddStudentsModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<AddStudentsModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Offerings_Offerings;
    [ViewData] public string PageTitle => "Bulk Add Students";

    [BindProperty(SupportsGet = true)]
    public OfferingId Id { get; set; } = OfferingId.Empty;

    [BindProperty(SupportsGet = true)] 
    public Grade? Grade { get; set; } = null;

    public string CourseName { get; set; }
    public string OfferingName { get; set; }

    [BindProperty]
    public List<StudentId> SelectedStudentIds { get; set; } = new();
    public List<StudentFromGradeResponse> Students { get; set; } = new();
    public List<EnrolmentResponse> ExistingEnrolments { get; set; } = new();

    public async Task OnGet() => await PreparePage();

    public async Task<IActionResult> OnPost()
    {
        EnrolMultipleStudentsInOfferingCommand command = new(Id, SelectedStudentIds);

        _logger
            .ForContext(nameof(EnrolMultipleStudentsInOfferingCommand), command, true)
            .Information("Requested to add bulk Students to Offering by user {User}", _currentUserService.UserName);

        Result request = await _mediator.Send(command);

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to add bulk Students to Offering by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                request.Error,
                _linkGenerator.GetPathByPage("/Subject/Offerings/Details", values: new { area = "Staff", Id = Id }));

            await PreparePage();

            return Page();
        }

        return RedirectToPage("/Subject/Offerings/Details", new { area = "Staff", Id = Id });
    }

    private async Task PreparePage()
    {
        _logger.Information("Requested to retrieve defaults to add bulk Students to Offering with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result<List<EnrolmentResponse>> enrolmentRequest = await _mediator.Send(new GetCurrentEnrolmentsForOfferingQuery(Id));

        if (enrolmentRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), enrolmentRequest.Error, true)
                .Warning("Failed to retrieve defaults to add bulk Students to Offering with id {Id} by user {User}", Id, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                enrolmentRequest.Error,
                _linkGenerator.GetPathByPage("/Subject/Offerings/Details", values: new { area = "Staff", Id = Id }));

            return;
        }

        ExistingEnrolments = enrolmentRequest.Value;

        if (Grade is null)
        {
            Result<List<StudentFromGradeResponse>> studentsRequest = await _mediator.Send(new GetStudentsFromOfferingGradeQuery(Id));

            if (studentsRequest.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), studentsRequest.Error, true)
                    .Warning("Failed to retrieve defaults to add bulk Students to Offering with id {Id} by user {User}", Id, _currentUserService.UserName);

                ModalContent = new ErrorDisplay(
                    studentsRequest.Error,
                    _linkGenerator.GetPathByPage("/Subject/Offerings/Details", values: new { area = "Staff", Id = Id }));

                return;
            }

            Students = studentsRequest.Value;
        }
        else
        {
            Result<List<StudentResponse>> studentsRequest = await _mediator.Send(new GetCurrentStudentsFromGradeQuery(Grade.Value));

            if (studentsRequest.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), studentsRequest.Error, true)
                    .Warning("Failed to retrieve defaults to add bulk Students to Offering with id {Id} by user {User}", Id, _currentUserService.UserName);

                ModalContent = new ErrorDisplay(
                    studentsRequest.Error,
                    _linkGenerator.GetPathByPage("/Subject/Offerings/Details", values: new { area = "Staff", Id = Id }));

                return;
            }

            Students = studentsRequest.Value
                .Select(entry => 
                    new StudentFromGradeResponse(entry.StudentId, entry.Name))
                .ToList();
        }

        Result<OfferingSummaryResponse> offeringRequest = await _mediator.Send(new GetOfferingSummaryQuery(Id));

        if (offeringRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), offeringRequest.Error, true)
                .Warning("Failed to retrieve defaults to add bulk Students to Offering with id {Id} by user {User}", Id, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                offeringRequest.Error,
                _linkGenerator.GetPathByPage("/Subject/Offerings/Details", values: new { area = "Staff", Id = Id }));

            return;
        }

        CourseName = offeringRequest.Value.CourseName;
        OfferingName = offeringRequest.Value.Name;
    }
}