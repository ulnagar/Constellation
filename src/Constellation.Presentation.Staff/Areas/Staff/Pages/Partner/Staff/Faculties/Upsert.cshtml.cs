namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Staff.Faculties;

using Application.Common.PresentationModels;
using Constellation.Application.Domains.Faculties.Commands.CreateFaculty;
using Constellation.Application.Domains.Faculties.Commands.UpdateFaculty;
using Constellation.Application.Domains.Faculties.Queries.GetFaculty;
using Constellation.Application.Models.Auth;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using Core.Abstractions.Services;
using Core.Models.Faculties.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.CanEditFaculties)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public UpsertModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<UpsertModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Staff_Faculties;
    [ViewData] public string PageTitle { get; set; } = "New Faculty";

    [BindProperty(SupportsGet = true)]
    public FacultyId Id { get; set; } = FacultyId.Empty;

    [Required]
    [BindProperty]
    public string Name { get; set; }
    [Required]
    [BindProperty]
    public string Colour { get; set; }

    public async Task OnGet()
    {
        if (Id != FacultyId.Empty)
        {
            _logger.Information("Requested to retrieve Faculty with id {FacultyId} for edit by user {User}", Id, _currentUserService.UserName);

            FacultyId facultyId = FacultyId.FromValue(Id.Value);
            
            // Get values from database
            Result<FacultyResponse> request = await _mediator.Send(new GetFacultyQuery(facultyId));

            if (request.IsFailure)
            {
                ModalContent = ErrorDisplay.Create(request.Error);

                _logger
                    .ForContext(nameof(Error), request.Error, true)
                    .Warning("Failed to retrieve Faculty with id {FacultyId} for edit by user {User}", Id, _currentUserService.UserName);

                return;
            }

            Name = request.Value.Name;
            Colour = request.Value.Colour;

            PageTitle = $"Editing {request.Value.Name}";
        }
    }

    public async Task<IActionResult> OnPostCreate()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Create new entry
        CreateFacultyCommand command = new(
            Name,
            Colour);

        _logger
            .ForContext(nameof(CreateFacultyCommand), command, true)
            .Information("Requested to create Faculty by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to create Faculty by user {User}", _currentUserService.UserName);
    
        return RedirectToPage("/Partner/Staff/Faculties/Index", new { area = "Staff"});
    }

    public async Task<IActionResult> OnPostUpdate()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        UpdateFacultyCommand command = new(
            Id,
            Name,
            Colour);

        _logger
            .ForContext(nameof(UpdateFacultyCommand), command, true)
            .Information("Requested to update Faculty with Id {FacultyId} by user {User}", Id, _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to update Faculty with Id {FacultyId} by user {User}", Id, _currentUserService.UserName);
        
        return RedirectToPage("/Partner/Staff/Faculties/Index", new { area = "Staff"});
    }
}
