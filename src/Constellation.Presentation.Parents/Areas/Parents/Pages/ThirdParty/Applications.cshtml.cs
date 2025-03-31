namespace Constellation.Presentation.Parents.Areas.Parents.Pages.ThirdParty;

using Application.Models.Auth;
using Application.ThirdPartyConsent.CreateTransaction;
using Application.ThirdPartyConsent.GetRequiredApplicationsForStudent;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Students.GetStudentsByParentEmail;
using Constellation.Core.Models.Families.Errors;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Logging;
using Core.Abstractions.Services;
using Core.Models.ThirdPartyConsent.Enums;
using Core.Models.ThirdPartyConsent.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Serilog;

[Authorize(Policy = AuthPolicies.IsParent)]
public class ApplicationsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public ApplicationsModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<ApplicationsModel>()
            .ForContext(LogDefaults.Application, LogDefaults.ParentPortal);
    }

    [ViewData] public string ActivePage => Models.ActivePage.ThirdParty;

    [BindProperty(SupportsGet = true)]
    public StudentId StudentId { get; set; } = StudentId.Empty;

    public StudentResponse? SelectedStudent { get; set; }
    public List<StudentResponse> Students { get; set; } = new();

    public List<RequiredApplicationResponse> Applications { get; set; } = new();

    [BindProperty]
    public Dictionary<string, bool> Responses { get; set; } = new();

    public async Task OnGet() => await PreparePage();

    public async Task<IActionResult> OnPost()
    {
        CreateTransactionCommand command = new(
            StudentId,
            _currentUserService.UserName,
            _currentUserService.EmailAddress,
            ConsentMethod.Portal, 
            string.Empty,
            Responses.ToDictionary(k => ApplicationId.FromValue(Guid.Parse(k.Key)), k => k.Value));

        _logger
            .ForContext(nameof(CreateTransactionCommand), command, true)
            .Information("Requested to submit Consent Responses by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(CreateTransactionCommand), command, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to submit Consent Responses by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(result.Error);

            return Page();
        }

        return RedirectToPage("/ThirdParty/Index", new { area = "Parents"});
    }

    public async Task<IActionResult> PreparePage()
    {
        _logger.Information("Requested to retrieve student list by user {user}", _currentUserService.UserName);

        Result<List<StudentResponse>> studentsRequest = await _mediator.Send(new GetStudentsByParentEmailQuery(_currentUserService.EmailAddress));

        if (studentsRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(studentsRequest.Error);

            return Page();
        }

        Students = studentsRequest.Value
            .Where(student => student.ResidentialFamily)
            .OrderBy(student => student.CurrentGrade)
            .ThenBy(student => student.LastName)
            .ThenBy(student => student.FirstName)
            .ToList();

        if (Students.Count == 0)
        {
            // Parent is non-residential and should not have access to this page
            return RedirectToPage("/Index", new { area = "Parents" });
        }

        if (Students.Count == 1)
            StudentId = Students.First().StudentId;

        if (StudentId != StudentId.Empty)
        {
            SelectedStudent = Students.FirstOrDefault(entry => entry.StudentId == StudentId);
            
            _logger.Information("Requested to retrieve consent details by user {user} for student {student}", _currentUserService.UserName, StudentId);

            Result<List<RequiredApplicationResponse>> applications = await _mediator.Send(new GetRequiredApplicationsForStudentQuery(StudentId));

            if (applications.IsFailure)
            {
                return Page();
            }

            Applications = applications.Value;
        }
    }
}