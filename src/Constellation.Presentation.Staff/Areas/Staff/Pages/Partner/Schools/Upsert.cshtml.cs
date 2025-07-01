namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Schools;

using Application.Common.PresentationModels;
using Application.Domains.Schools.Commands.UpsertSchool;
using Application.Domains.Schools.Queries.GetSchoolForEdit;
using Application.Models.Auth;
using Core.Abstractions.Services;
using Core.Errors;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.CanEditSchools)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public UpsertModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<UpsertModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Schools_Schools;
    [ViewData] public string PageTitle { get; set; } = "New School";

    [BindProperty(SupportsGet = true)]
    public string? Id { get; set; } = null;

    [BindProperty] 
    public string SchoolCode { get; set; } = string.Empty;

    [BindProperty]
    public string Name { get; set; } = string.Empty;

    [BindProperty]
    public string Address { get; set; } = string.Empty;

    [BindProperty]
    public string Town { get; set; } = string.Empty;

    [BindProperty]
    public string State { get; set; } = string.Empty;

    [BindProperty]
    public string PostCode { get; set; } = string.Empty;

    [BindProperty]
    public string PhoneNumber { get; set; } = string.Empty;

    [BindProperty]
    public string? FaxNumber { get; set; } = string.Empty;

    [BindProperty]
    public string EmailAddress { get; set; } = string.Empty;

    [BindProperty]
    public string? Division { get; set; } = string.Empty;

    [BindProperty]
    public bool HeatSchool { get; set; }

    [BindProperty]
    public string? Electorate { get; set; } = string.Empty;

    [BindProperty]
    public string? PrincipalNetwork { get; set; } = string.Empty;

    public async Task OnGet()
    {
        if (Id is null)
            return;

        _logger
            .Information("Requested to retrieve School with id {Id} for edit by user {User}", Id, _currentUserService.UserName);

        Result<SchoolEditResponse> school = await _mediator.Send(new GetSchoolForEditQuery(Id));

        if (school.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                school.Error,
                _linkGenerator.GetPathByPage("/Partner/Schools/Index", values: new { area = "Staff" }));

            _logger
                .ForContext(nameof(Error), school.Error, true)
                .Information("Failed to retrieve School with id {Id} for edit by user {User}", Id, _currentUserService.UserName);
            
            return;
        }

        SchoolCode = school.Value.SchoolCode;
        Name = school.Value.Name;
        Address = school.Value.Address;
        Town = school.Value.Town;
        State = school.Value.State;
        PostCode = school.Value.PostCode;
        PhoneNumber = school.Value.PhoneNumber;
        FaxNumber = school.Value.FaxNumber;
        EmailAddress = school.Value.EmailAddress;
        Division = school.Value.Division;
        HeatSchool = school.Value.HeatSchool;
        Electorate = school.Value.Electorate;
        PrincipalNetwork = school.Value.PrincipalNetwork;

        PageTitle = $"Editing {school.Value.Name}";
    }

    public async Task<IActionResult> OnPost()
    {
        string phoneNumber = PhoneNumber
            .Replace("(", string.Empty)
            .Replace(")", string.Empty)
            .Replace(" ", string.Empty)
            .Trim();

        string faxNumber = FaxNumber?
            .Replace("(", string.Empty)
            .Replace(")", string.Empty)
            .Replace(" ", string.Empty)
            .Trim()
            ?? string.Empty;

        UpsertSchoolCommand command = new()
        {
            Code = SchoolCode,
            Name = Name,
            Address = Address,
            Town = Town,
            State = State,
            PostCode = PostCode,
            PhoneNumber = phoneNumber,
            FaxNumber = faxNumber,
            EmailAddress = EmailAddress,
            Division = Division,
            LateOpening = HeatSchool,
            Electorate = Electorate,
            PrincipalNetwork = PrincipalNetwork
        };

        _logger
            .ForContext(nameof(UpsertSchoolCommand), command, true)
            .Information("Requested to upsert School by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(result.Error);

            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Information("Failed to upsert School by user {User}", _currentUserService.UserName);

            return Page();
        }

        return RedirectToPage("/Partner/Schools/Index", new { area = "Staff" });
    }
}