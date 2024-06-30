namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Compliance.MasterFile;

using Constellation.Application.Common.PresentationModels;
using Constellation.Application.ExternalDataConsistency;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Staff.Areas;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public IndexModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Compliance_MasterFile;

    [BindProperty]
    public IFormFile? FormFile { get; set; }

    [BindProperty]
    public bool EmailReport { get; set; }

    public List<UpdateItem> UpdateItems { get; set; } = new();

    public async Task OnGet() { }

    public async Task<IActionResult> OnPost(CancellationToken cancellationToken)
    {
        if (FormFile is not null)
        {
            try
            {
                string? emailAddress = string.Empty;

                if (EmailReport)
                {
                    emailAddress = User.Identity?.Name;

                    if (emailAddress is null)
                        EmailReport = false;
                }

                await using var target = new MemoryStream();
                await FormFile.CopyToAsync(target, cancellationToken);

                Result<List<UpdateItem>> outputRequest = await _mediator.Send(new MasterFileConsistencyCoordinator(target, EmailReport, emailAddress), cancellationToken);

                if (outputRequest.IsFailure)
                {
                    Error = new ErrorDisplay
                    {
                        Error = outputRequest.Error,
                        RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Compliance/MasterFile/Index", values: new { area = "Staff" })
                    };

                    return Page();
                }

                UpdateItems = outputRequest.Value;
            }
            catch (Exception ex)
            {
                Error = new ErrorDisplay
                {
                    Error = new Error("Exception", ex.Message),
                    RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Compliance/MasterFile/Index", values: new { area = "Staff" })
                };

                return Page();
            }
        }

        return Page();
    }
}