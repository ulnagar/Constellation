namespace Constellation.Presentation.Server.Areas.Reports.Pages;

using Constellation.Application.ExternalDataConsistency;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class MasterFileModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public MasterFileModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [BindProperty]
    public IFormFile FormFile { get; set; }
    [BindProperty]
    public bool EmailReport { get; set; }

    public List<UpdateItem> UpdateItems { get; set; } = new();

    public async Task OnGet()
    {
        await GetClasses(_mediator);
    }

    public async Task<IActionResult> OnPost(CancellationToken cancellationToken)
    {
        if (FormFile is not null)
        {
            try
            {
                string emailAddress = string.Empty;

                if (EmailReport)
                {
                    emailAddress = User.Identity?.Name;

                    if (emailAddress is null)
                        EmailReport = false;
                }

                await using var target = new MemoryStream();
                await FormFile.CopyToAsync(target, cancellationToken);

                var outputRequest = await _mediator.Send(new MasterFileConsistencyCoordinator(target, EmailReport, emailAddress), cancellationToken);

                if (outputRequest.IsFailure)
                {
                    Error = new ErrorDisplay
                    {
                        Error = outputRequest.Error,
                        RedirectPath = _linkGenerator.GetPathByPage("MasterFile", values: new { area = "Reports" })
                    };

                    return Page();
                }

                UpdateItems = outputRequest.Value;
            }
            catch (Exception ex)
            {
                Error = new ErrorDisplay
                {
                    Error = new Core.Shared.Error("Exception", ex.Message),
                    RedirectPath = _linkGenerator.GetPathByPage("MasterFile", values: new { area = "Reports" })
                };

                return Page();
            }
        }

        return Page();
    }
}
