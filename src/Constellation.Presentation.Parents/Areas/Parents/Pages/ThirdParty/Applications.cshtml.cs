namespace Constellation.Presentation.Parents.Areas.Parents.Pages.ThirdParty;

using Application.Models.Auth;
using Application.ThirdPartyConsent.CreateTransaction;
using Application.ThirdPartyConsent.GetRequiredApplicationsForStudent;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Students.GetStudentsByParentEmail;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Shared;
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
            .ForContext("APPLICATION", "Parent Portal")
            .ForContext<ApplicationsModel>();
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
        Result result = await _mediator.Send(new CreateTransactionCommand(
            StudentId,
            _currentUserService.EmailAddress,
            ConsentMethod.Portal, 
            string.Empty,
            Responses.ToDictionary(k => ApplicationId.FromValue(Guid.Parse(k.Key)), k => k.Value)));

        if (result.IsFailure)
        {
            // Log this
            return Page();
        }

        return RedirectToPage();
    }

    public async Task PreparePage()
    {
        _logger.Information("Requested to retrieve student list by user {user}", _currentUserService.UserName);

        Result<List<StudentResponse>> studentsRequest =
            await _mediator.Send(new GetStudentsByParentEmailQuery(_currentUserService.EmailAddress));

        if (studentsRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(studentsRequest.Error);

            return;
        }

        Students = studentsRequest.Value
            .OrderBy(student => student.CurrentGrade)
            .ThenBy(student => student.LastName)
            .ThenBy(student => student.FirstName)
            .ToList();

        if (Students.Count == 1)
            StudentId = Students.First().StudentId;

        if (StudentId != StudentId.Empty)
        {
            SelectedStudent = Students.FirstOrDefault(entry => entry.StudentId == StudentId);
            
            _logger.Information("Requested to retrieve consent details by user {user} for student {student}", _currentUserService.UserName, StudentId);

            Result<List<RequiredApplicationResponse>> applications = await _mediator.Send(new GetRequiredApplicationsForStudentQuery(StudentId));

            if (applications.IsFailure)
            {
                return;
            }

            Applications = applications.Value;
        }
    }
}