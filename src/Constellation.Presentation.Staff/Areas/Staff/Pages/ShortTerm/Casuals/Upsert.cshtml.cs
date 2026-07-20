namespace Constellation.Presentation.Staff.Areas.Staff.Pages.ShortTerm.Casuals;

using Application.Domains.Casuals.Commands.CreateCasual;
using Application.Domains.Casuals.Commands.UpdateCasual;
using Application.Domains.Casuals.Queries.GetCasualById;
using Application.Domains.Schools.Models;
using Application.Domains.Schools.Queries.GetSchoolsForSelectionList;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Identifiers;
using Constellation.Presentation.Shared.Helpers.Attributes;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Extensions;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Threading;

[HasPermission(AuthPermission.ShortTerm_Casuals_Edit_Value)]
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
            .ForStaffPortal();
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.ShortTerm_Casuals_Index;
    [ViewData] public string PageTitle { get; set; } = "New Casual Teacher";


    [BindProperty(SupportsGet = true)]
    public CasualId Id { get; set; } = CasualId.Empty;
    
    [BindProperty]
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    public string LastName { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [DataType(DataType.EmailAddress)]
    public string EmailAddress { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    public SchoolCode SchoolCode { get; set; } = SchoolCode.Empty;
    
    public List<SchoolSelectionListResponse> Schools { get; set; } = new();

    public async Task OnGet(CancellationToken cancellationToken)
    {
        if (Id != CasualId.Empty)
        {
            Result<CasualResponse> casualResponse = await _mediator.Send(new GetCasualByIdQuery(Id), cancellationToken);

            if (casualResponse.IsFailure)
            {
                ModalContent = ErrorDisplay.Create(
                    casualResponse.Error,
                    _linkGenerator.GetPathByPage("/ShortTerm/Casuals/Index", values: new { area = "Staff" }));

                return;
            }

            FirstName = casualResponse.Value.FirstName;
            LastName = casualResponse.Value.LastName;
            EmailAddress = casualResponse.Value.EmailAddress;
            SchoolCode = casualResponse.Value.SchoolCode;
        }

        await PreparePage(cancellationToken);
    }

    public async Task<IActionResult> OnPostCreate(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PreparePage(cancellationToken);
            return Page();
        }

        CreateCasualCommand command = new(
            FirstName,
            LastName,
            EmailAddress,
            SchoolCode,
            string.Empty);

        _logger
            .ForContext(nameof(CreateCasualCommand), command, true)
            .Information("Requested to create Casual Teacher by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to create Casual Teacher by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                result.Error,
                _linkGenerator.GetPathByPage("/ShortTerm/Casuals/Index", values: new { area = "Staff" }));

            await PreparePage(cancellationToken);

            return Page();
        }

        return RedirectToPage("/ShortTerm/Casuals/Index", new { area = "Staff" });
    }

    public async Task<IActionResult> OnPostUpdate(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PreparePage(cancellationToken);
            return Page();
        }

        UpdateCasualCommand command = new(
            Id,
            FirstName,
            LastName,
            EmailAddress,
            SchoolCode,
            string.Empty);

        _logger
            .ForContext(nameof(UpdateCasualCommand), command, true)
            .Information("Requested to update Casual Teacher with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to update Casual Teacher with id {Id} by user {User}", Id, _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                result.Error,
                _linkGenerator.GetPathByPage("/ShortTerm/Casuals/Index", values: new { area = "Staff" }));

            await PreparePage(cancellationToken);
            
            return Page();
        }

        return RedirectToPage("/ShortTerm/Casuals/Index", new { area = "Staff" });
    }

    private async Task PreparePage(CancellationToken cancellationToken = default)
    {
        Result<List<SchoolSelectionListResponse>> schoolsResponse = await _mediator.Send(new GetSchoolsForSelectionListQuery(), cancellationToken);

        if (schoolsResponse.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                schoolsResponse.Error,
                _linkGenerator.GetPathByPage("/ShortTerm/Casuals/Index", values: new { area = "Staff" }));

            return;
        }

        Schools = schoolsResponse.Value;
    }
}
