namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Constellation.Application.ExternalDataConsistency;
using Constellation.Application.Features.Common.Queries;
using Constellation.Application.Features.MandatoryTraining.Queries;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.MandatoryTraining;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;

    public IndexModel(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty]
    public IFormFile FormFile { get; set; }
    public List<UpdateItem> UpdateItems { get; set; } = new();

    public async Task OnGetAsync()
    {
        await GetClasses(_mediator);
    }

    public async Task<IActionResult> OnPostCheckFile()
    {
        if (FormFile is not null)
        {
            try
            {
                await using var target = new MemoryStream();
                await FormFile.CopyToAsync(target);

                var outputRequest = await _mediator.Send(new MasterFileConsistencyCoordinator(target));

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
