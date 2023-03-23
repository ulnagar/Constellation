namespace Constellation.Presentation.Server.Areas.ShortTerm.Pages.Casuals;

using Constellation.Application.Casuals.CreateCasual;
using Constellation.Application.Casuals.GetCasualById;
using Constellation.Application.Casuals.UpdateCasual;
using Constellation.Application.Models.Auth;
using Constellation.Application.Schools.GetSchoolsForSelectionList;
using Constellation.Application.Schools.Models;
using Constellation.Core.Models.Identifiers;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    [BindProperty]
    public string AdobeConnectId { get; set; } = string.Empty;

    public List<SchoolSelectionListResponse> Schools { get; set; } = new();

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
    {
        await GetClasses(_mediator);

        if (Id.HasValue)
        {
            var casualResponse = await _mediator.Send(new GetCasualByIdQuery(CasualId.FromValue(Id.Value)), cancellationToken);

            if (casualResponse.IsFailure)
            {
                Error = new ErrorDisplay
                {
                    Error = casualResponse.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/Casuals/Index", values: new { area = "ShortTerm" })
                };

                return Page();
            }

            FirstName = casualResponse.Value.FirstName;
            LastName = casualResponse.Value.LastName;
            EmailAddress = casualResponse.Value.EmailAddress;
            SchoolCode = casualResponse.Value.SchoolCode;
            AdobeConnectId = casualResponse.Value.AdobeConnectId;
        }

        var schoolsResponse = await _mediator.Send(new GetSchoolsForSelectionListQuery(), cancellationToken);

        if (schoolsResponse.IsFailure)
        {
            Error = new ErrorDisplay
            {
                Error = schoolsResponse.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Casuals/Index", values: new { area = "ShortTerm" })
            };

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
                        AdobeConnectId),
                    cancellationToken);

                if (result.IsSuccess)
                {
                    Error = new ErrorDisplay
                    {
                        Error = result.Error,
                        RedirectPath = _linkGenerator.GetPathByPage("/Casuals/Index", values: new { area = "ShortTerm" })
                    };

                    return Page();
                }

                ModelState.AddModelError("", result.Error.Message);
            } 
            else
            {
                var result = await _mediator.Send(
                    new CreateCasualCommand(
                        FirstName,
                        LastName,
                        EmailAddress,
                        SchoolCode,
                        AdobeConnectId),
                    cancellationToken);

                if (result.IsSuccess)
                {
                    Error = new ErrorDisplay
                    {
                        Error = result.Error,
                        RedirectPath = _linkGenerator.GetPathByPage("/Casuals/Index", values: new { area = "ShortTerm" })
                    };

                    return Page();
                }

                ModelState.AddModelError("", result.Error.Message);
            }
        }

        var schoolsResponse = await _mediator.Send(new GetSchoolsForSelectionListQuery(), cancellationToken);

        if (schoolsResponse.IsFailure)
        {
            Error = new ErrorDisplay
            {
                Error = schoolsResponse.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Casuals/Index", values: new { area = "ShortTerm" })
            };

            return Page();
        }

        Schools = schoolsResponse.Value;

        return Page();
    }
}
