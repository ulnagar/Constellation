namespace Constellation.Presentation.Staff.Areas.Staff.Pages.ShortTerm.Casuals;

using Constellation.Application.Casuals.CreateCasual;
using Constellation.Application.Casuals.GetCasualById;
using Constellation.Application.Casuals.UpdateCasual;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Models.Auth;
using Constellation.Application.Schools.GetSchoolsForSelectionList;
using Constellation.Application.Schools.Models;
using Constellation.Core.Models.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.CanEditCasuals)]
public class UpsertModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public UpsertModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.ShortTerm_Casuals_Index;


    [BindProperty(SupportsGet = true)]
    public Guid? Id { get; set; }
    
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
    public string SchoolCode { get; set; } = string.Empty;
    
    public List<SchoolSelectionListResponse> Schools { get; set; } = new();

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
    {
        if (Id.HasValue)
        {
            var casualResponse = await _mediator.Send(new GetCasualByIdQuery(CasualId.FromValue(Id.Value)), cancellationToken);

            if (casualResponse.IsFailure)
            {
                ModalContent = new ErrorDisplay(
                    casualResponse.Error,
                    _linkGenerator.GetPathByPage("/ShortTerm/Casuals/Index", values: new { area = "Staff" }));

                return Page();
            }

            FirstName = casualResponse.Value.FirstName;
            LastName = casualResponse.Value.LastName;
            EmailAddress = casualResponse.Value.EmailAddress;
            SchoolCode = casualResponse.Value.SchoolCode;
        }

        var schoolsResponse = await _mediator.Send(new GetSchoolsForSelectionListQuery(), cancellationToken);

        if (schoolsResponse.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                schoolsResponse.Error,
                _linkGenerator.GetPathByPage("/ShortTerm/Casuals/Index", values: new { area = "Staff" }));

            return Page();
        }

        Schools = schoolsResponse.Value;

        return Page();
    }

    public async Task<IActionResult> OnPostUpdate(CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            if (Id.HasValue)
            {
                var result = await _mediator.Send(
                    new UpdateCasualCommand(
                        CasualId.FromValue(Id.Value),
                        FirstName,
                        LastName,
                        EmailAddress,
                        SchoolCode,
                        string.Empty),
                    cancellationToken);

                if (result.IsFailure)
                {
                    ModalContent = new ErrorDisplay(
                        result.Error,
                        _linkGenerator.GetPathByPage("/ShortTerm/Casuals/Index", values: new { area = "Staff" }));

                    return Page();
                }
            } 
            else
            {
                var result = await _mediator.Send(
                    new CreateCasualCommand(
                        FirstName,
                        LastName,
                        EmailAddress,
                        SchoolCode,
                        string.Empty),
                    cancellationToken);

                if (result.IsFailure)
                {
                    ModalContent = new ErrorDisplay(
                        result.Error,
                        _linkGenerator.GetPathByPage("/ShortTerm/Casuals/Index", values: new { area = "Staff" }));

                    return Page();
                }
            }

            return RedirectToPage("/Casuals/Index", new { area = "ShortTerm" });
        }

        var schoolsResponse = await _mediator.Send(new GetSchoolsForSelectionListQuery(), cancellationToken);

        if (schoolsResponse.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                schoolsResponse.Error,
                _linkGenerator.GetPathByPage("/ShortTerm/Casuals/Index", values: new { area = "Staff" }));

            return Page();
        }

        Schools = schoolsResponse.Value;

        return Page();
    }
}
