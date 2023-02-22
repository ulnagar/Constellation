namespace Constellation.Presentation.Server.Areas.ShortTerm.Pages.Casuals;

using Constellation.Application.Casuals.GetCasualById;
using Constellation.Application.Models.Auth;
using Constellation.Application.Schools.GetSchoolsForSelectionList;
using Constellation.Application.Schools.Models;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.CanEditCasuals)]
public class UpsertModel : BasePageModel
{
    private readonly IMediator _mediator;

    public UpsertModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty(SupportsGet = true)]
    public Guid? Id { get; set; }
    
    [BindProperty]
    [Required]
    public string FirstName { get; set; }
    
    [BindProperty]
    [Required]
    public string LastName { get; set; }
    
    [BindProperty]
    [Required]
    [DataType(DataType.EmailAddress)]
    public string EmailAddress { get; set; }

    [BindProperty]
    [Required]
    public string SchoolCode { get; set; }

    [BindProperty]
    public string AdobeConnectId { get; set; }

    public List<SchoolSelectionListResponse> Schools { get; set; }

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
    {
        await GetClasses(_mediator);

        if (Id.HasValue)
        {
            var casualResponse = await _mediator.Send(new GetCasualByIdQuery(Id.Value), cancellationToken);

            if (casualResponse.IsFailure)
            {
                return RedirectToAction("Index");
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
            return RedirectToAction("Index");
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
                        Id,
                        FirstName,
                        LastName,
                        EmailAddress,
                        SchoolCode,
                        AdobeConnectId),
                    cancellationToken);

                if (result.IsSuccess)
                {
                    return RedirectToPage("Index");
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
                    return RedirectToPage("Index");
                }

                ModelState.AddModelError("", result.Error.Message);
            }
        }

        var schoolsResponse = await _mediator.Send(new GetSchoolsForSelectionListQuery(), cancellationToken);

        if (schoolsResponse.IsFailure)
        {
            return RedirectToAction("Index");
        }

        Schools = schoolsResponse.Value;

        return Page();
    }
}
