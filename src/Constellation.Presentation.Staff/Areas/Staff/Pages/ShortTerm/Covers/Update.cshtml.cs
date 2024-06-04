namespace Constellation.Presentation.Staff.Areas.Staff.Pages.ShortTerm.Covers;

using Constellation.Application.ClassCovers.GetCoverWithDetails;
using Constellation.Application.ClassCovers.UpdateCover;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.ComponentModel.DataAnnotations;
using System.Threading;

[Authorize(Policy = AuthPolicies.CanEditCovers)]
public class UpdateModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public UpdateModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Constellation.Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.ShortTerm_Covers_Index;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }
    public string OfferingName { get; set; }
    [BindProperty]
    [DataType(DataType.Date)]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
    public DateOnly StartDate { get; set; }
    [BindProperty]
    [DataType(DataType.Date)]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
    public DateOnly EndDate { get; set; }
    public string TeacherSchool { get; set; }
    public string TeacherName { get; set; }

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
    {
        await PreparePage(cancellationToken);

        return Page();
    }

    public async Task<IActionResult> OnPostUpdate(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateCoverCommand(ClassCoverId.FromValue(Id), StartDate, EndDate), cancellationToken);

        if (result.IsFailure)
        {
            await PreparePage(cancellationToken);

            ModelState.AddModelError("", result.Error.Message);

            return Page();
        }

        return RedirectToPage("Index");
    }

    private async Task<bool> PreparePage(CancellationToken cancellationToken = default)
    {
        var coverResult = await _mediator.Send(new GetCoverWithDetailsQuery(ClassCoverId.FromValue(Id)), cancellationToken);

        if (coverResult.IsFailure)
        {
            Error = new ErrorDisplay
            {
                Error = coverResult.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/ShortTerm/Covers/Index", values: new { area = "Staff" })
            };

            return false;
        }

        OfferingName = coverResult.Value.OfferingName;
        StartDate = coverResult.Value.StartDate;
        EndDate = coverResult.Value.EndDate;
        TeacherName = coverResult.Value.UserName;
        TeacherSchool = coverResult.Value.UserSchool;

        return true;
    }
}
