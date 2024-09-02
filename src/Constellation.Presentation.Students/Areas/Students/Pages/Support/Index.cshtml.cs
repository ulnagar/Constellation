namespace Constellation.Presentation.Students.Areas.Students.Pages.Support;

using Application.Models.Auth;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Contacts.GetContactListForParentPortal;
using Constellation.Application.Students.GetStudentsByParentEmail;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
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
            .ForContext(StudentLogDefaults.Application, StudentLogDefaults.StudentPortal);
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
}