namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Contacts;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Application.SchoolContacts.CreateContactWithRole;
using Application.Schools.GetSchoolById;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
public class CreateModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public CreateModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger,
        IHttpContextAccessor httpContextAccessor, 
        IServiceScopeFactory serviceFactory) 
        : base(httpContextAccessor, serviceFactory)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<CreateModel>()
            .ForContext("Application", "Schools Portal");
    }

    [ViewData] public string ActivePage => Models.ActivePage.Contacts;


    [BindProperty]
    public string FirstName { get; set; }

    [BindProperty]
    public string LastName { get; set; }

    [BindProperty]
    public string? PhoneNumber { get; set; }

    [BindProperty]
    public string EmailAddress { get; set; }

    [BindProperty]
    public string Position { get; set; }

    [BindProperty]
    public string SchoolCode { get; set; }

    public string SchoolName { get; set; }

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve school data by user {user} for school {school}", _currentUserService.UserName, CurrentSchoolCode);
        
        Result<SchoolResponse> request = await _mediator.Send(new GetSchoolByIdQuery(CurrentSchoolCode));

        if (request.IsFailure)
        {
            ModalContent = new ErrorDisplay(request.Error);

            return;
        }

        SchoolName = request.Value.Name;
        SchoolCode = request.Value.SchoolCode;
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            Result<SchoolResponse> request = await _mediator.Send(new GetSchoolByIdQuery(CurrentSchoolCode));

            SchoolName = request.Value.Name;
            SchoolCode = request.Value.SchoolCode;

            return Page();
        }

        CreateContactWithRoleCommand command = new(
            FirstName,
            LastName,
            EmailAddress,
            PhoneNumber,
            Position,
            SchoolCode,
            string.Empty,
            true);

        _logger.Information("Requested to create new school contact by user {user} with data {@command}", _currentUserService.UserName, command);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            ModalContent = new ErrorDisplay(result.Error);

            return Page();
        }

        return RedirectToPage("/Contacts/Index", new { area = "Schools" });
    }
}