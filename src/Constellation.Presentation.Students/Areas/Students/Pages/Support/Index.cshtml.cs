namespace Constellation.Presentation.Students.Areas.Students.Pages.Support;

using Application.Domains.Contacts.Queries.GetContactListForParentPortal;
using Application.Domains.ExternalSystems.HelpDesk.Commands.SubmitSupportTicket;
using Application.Domains.Students.Queries.GetStudentById;
using Application.Models.Auth;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.Students.Models;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Logging;
using Core.Abstractions.Services;
using Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStudent)]
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
            .ForContext(LogDefaults.Application, LogDefaults.StudentPortal);
    }
    
    [ViewData] public string ActivePage => Models.ActivePage.Contacts;

    public List<StudentSupportContactResponse> Contacts { get; set; } = new();

    public async Task OnGet() => await PreparePage();

    public async Task PreparePage()
    {
        _logger.Information("Requested to retrieve support contacts by user {user}", _currentUserService.UserName);

        string studentIdClaimValue = User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StudentId)?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(studentIdClaimValue))
        {
            _logger
                .ForContext(nameof(Error), StudentErrors.InvalidId, true)
                .Warning("Failed to retrieve support contacts by user {user}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(StudentErrors.InvalidId);

            return;
        }

        StudentId studentId = StudentId.FromValue(new(studentIdClaimValue));
        
        Result<List<StudentSupportContactResponse>> contactsRequest = await _mediator.Send(new GetContactListForParentPortalQuery(studentId));

        if (contactsRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), contactsRequest.Error, true)
                .Warning("Failed to retrieve support contacts by user {user}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(contactsRequest.Error);

            return;
        }

        Contacts = contactsRequest.Value;
    }

    public async Task<IActionResult> OnPostTechnologyForm(
        string issueType,
        string serialNumber,
        string description)
    {
        string studentIdClaimValue = User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StudentId)?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(studentIdClaimValue))
        {
            _logger
                .ForContext(nameof(Error), StudentErrors.InvalidId, true)
                .Warning("Failed to retrieve support contacts by user {user}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(StudentErrors.InvalidId);

            return Page();
        }

        StudentId studentId = StudentId.FromValue(new(studentIdClaimValue));

        Result<StudentResponse> student = await _mediator.Send(new GetStudentByIdQuery(studentId));

        if (student.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), student.Error, true)
                .Warning("Failed to submit student support request by user {user}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(student.Error);

            return Page();
        }

        Result<EmailRecipient> recipient = EmailRecipient.Create(student.Value.Name.DisplayName, student.Value.EmailAddress.Email);

        if (recipient.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), recipient.Error, true)
                .Warning("Failed to submit student support request by user {user}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(recipient.Error);

            return Page();
        }
        
        SubmitSupportTicketCommand command = new(
            recipient.Value,
            issueType,
            serialNumber,
            description);

        _logger
            .ForContext(nameof(SubmitSupportTicketCommand), command, true)
            .Information("Requested to submit student support request by user {user}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to submit student support request by user {user}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(result.Error);

            return Page();
        }

        ModalContent = new FeedbackDisplay(
            "Success",
            "Thank you. Your request for support has been sent to our Technology Support Team!",
            "Close",
            "btn-default",
            _linkGenerator.GetPathByPage("/Support/Index", values: new { area = "Students" }));

        return Page();
    }
}