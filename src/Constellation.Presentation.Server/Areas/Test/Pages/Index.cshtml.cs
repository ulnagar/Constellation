namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Constellation.Application.ExternalDataConsistency;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly IProcessOutboxMessagesJob _familySyncJob;

    public IndexModel(
        IMediator mediator,
        IProcessOutboxMessagesJob familySyncJob)
    {
        _mediator = mediator;
        _familySyncJob = familySyncJob;
    }

    [BindProperty]
    public IFormFile FormFile { get; set; }
    [BindProperty]
    public bool EmailReport { get; set; }

    public List<UpdateItem> UpdateItems { get; set; } = new();

    public async Task OnGetAsync()
    {
        await GetClasses(_mediator);
    }

    public async Task OnGetFamilyUpdate(CancellationToken cancellationToken = default)
    {
        await _familySyncJob.StartJob(Guid.NewGuid(), cancellationToken);
    }

    public async Task<IActionResult> OnPostCheckFile()
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
                await FormFile.CopyToAsync(target);

                var outputRequest = await _mediator.Send(new MasterFileConsistencyCoordinator(target, EmailReport, emailAddress));

                if (outputRequest.IsFailure)
                {
                    return NotFound();
                }

                UpdateItems = outputRequest.Value;
            }
            catch (Exception ex)
            {
                // Error uploading file
            }
        }

        return Page();
    }
}
